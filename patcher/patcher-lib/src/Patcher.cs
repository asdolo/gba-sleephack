using System.Buffers.Binary;
using static GBA.Sleephack.Constants;

namespace GBA.Sleephack;

public class Patcher
{
    private const int ARM_REGISTERS_COUNT = 16;
    private const uint DUMMY_DATA = 0xDEADBEEF;

    private const uint AGB_INTERRUPTION_HANDLER_LOCATION_ADDRESS = 0x03007FFC;

    public Patcher(byte[] romBinary, byte[] sleepPatchBinary)
    {
        ROMBinary = romBinary;
        SleepPatchBinary = sleepPatchBinary;
        
        // We start with a dummy patch address. We'll find a real one later...
        PatchAddress = DUMMY_DATA;
    }

    private byte[] ROMBinary { get; }
    private byte[] SleepPatchBinary { get; }
    private uint PatchAddress { get; set; }

    private int GetROMSize()
    {
        return ROMBinary.Length;
    }

    private int GetROMWordCount()
    {
        return GetROMSize() / ARM_WORD_SIZE;
    }

    private int GetROMHalfWordCount()
    {
        return GetROMSize() / ARM_HALF_WORD_SIZE;
    }

    public byte[] Patch()
    {
        // We need to find a suitable address in the ROM to inject both sleep patch and activation routines
        var address = FindSuitablePatchAddress();
        
        PatchAddress = (uint) address;

        var romBytesSpan = ROMBinary.AsSpan();

        // Add first Activation Patch just after the Sleep Patch
        var lastActivationPatchAddress = PatchAddress + SleepPatchBinary.Length;

        var romInstructionCount = GetROMWordCount();
        var activationPatchesData = new List<ActivationPatchData>();
        activationPatchesData.AddRange(GetActivationPatchesData(romInstructionCount, romBytesSpan,
            ref lastActivationPatchAddress, false));

        // YAY now for THUMB version!
        romInstructionCount = GetROMHalfWordCount();
        activationPatchesData.AddRange(GetActivationPatchesData(romInstructionCount, romBytesSpan,
            ref lastActivationPatchAddress, true));

        // Apply all patches

        var patchedROMSize = CalculatePatchedROMSize(activationPatchesData);
        var patchedROM = new byte[patchedROMSize];

        // Copy original ROM in the patched ROM bytes array
        Array.Copy(ROMBinary, 0, patchedROM, 0, GetROMSize());

        // Copy sleep patch to the indicated address in the patched ROM
        Array.Copy(SleepPatchBinary, 0, patchedROM, PatchAddress, SleepPatchBinary.Length);

        // Apply every activation patch to the patched ROM
        activationPatchesData.ForEach(activationPatchData => activationPatchData.ApplyTo(patchedROM));


        return patchedROM;
    }

    private int FindSuitablePatchAddress()
    {
        // A suitable address is any address in which we could fit the whole patch,
        // so first we need to know the whole size of the patches

        // We know the size of the sleep patch, and how long is an activation routine, but we don't know
        // how many activation routines we are going to inject if we don't scan the ROM first

        // So... let's calculate the total size of all the activation routines we'll inject
        var dummyAddress = (long) DUMMY_DATA;
        var activationPatchesSize =
            GetActivationPatchesData(GetROMWordCount(), ROMBinary.AsSpan(), ref dummyAddress, false)
                .Select(patchData => patchData.Patch.Length)
                .Sum() +
            GetActivationPatchesData(GetROMHalfWordCount(), ROMBinary.AsSpan(), ref dummyAddress, true)
                .Select(patchData => patchData.Patch.Length)
                .Sum();

        var totalPatchesSize = SleepPatchBinary.Length + activationPatchesSize;

        // Expand to next multiple of four words.
        totalPatchesSize = Utils.RoundUp(totalPatchesSize, 4 * ARM_WORD_SIZE);

        // With the total size of all the patches we can now scan the ROM for a suitable location to fit the whole thing
        try
        {
            return EmptyDataAddressFinder.For(ROMBinary).FindWithSize(totalPatchesSize);
        }
        catch (Exception e)
        {
            if (totalPatchesSize <= AGB_ROM_MAX_SIZE - GetROMSize())
                // Add patches to the end of the ROM
                return GetROMSize();
            
            throw new Exception("Could not find a suitable location in the ROM to inject the sleep patch.");
        }
    }

    private IEnumerable<ActivationPatchData> GetActivationPatchesData(int romInstructionCount, Span<byte> romBytesSpan,
        ref long lastActivationPatchAddress, bool thumb)
    {
        var activationPatchesData = new List<ActivationPatchData>();

        var instructionParser = (IARMInstructionParser) (thumb
            ? new ARMThumbInstructionParser()
            : new ARMInstructionParser());

        var instructionLength = thumb ? ARM_HALF_WORD_SIZE : ARM_WORD_SIZE;

        var lastDataReadToR = new uint[ARM_REGISTERS_COUNT];
        var lastDataAddressReadToR = new uint[ARM_REGISTERS_COUNT];
        var lastLDRInstructionAddress = new uint[ARM_REGISTERS_COUNT];

        for (var instructionIndex = 0; instructionIndex < romInstructionCount; instructionIndex++)
        {
            var instructionAddress = (uint) (instructionIndex * instructionLength);
            var instructionBytes = romBytesSpan.Slice((int) instructionAddress, instructionLength);
            var instruction = thumb
                ? BinaryPrimitives.ReadUInt16LittleEndian(instructionBytes)
                : BinaryPrimitives.ReadUInt32LittleEndian(instructionBytes);

            if (instructionParser.IsLDRInstruction(instruction))
            {
                var ldrDestinationRegister = instructionParser.GetLDRDestinationRegister(instruction);
                var ldrValueAddress = instructionParser.GetLDRValueAddress(instruction, instructionAddress);

                var value = DUMMY_DATA;

                if (thumb) ldrValueAddress = (uint) (ldrValueAddress & ~0x03);

                if (IsValidROMAddress(ldrValueAddress))
                    if (IsAlignedAddress(ldrValueAddress, thumb))
                        // Read value that is pointed in LDR instruction
                        value = BinaryPrimitives.ReadUInt32LittleEndian(
                            romBytesSpan.Slice((int) ldrValueAddress, ARM_WORD_SIZE)
                        );

                // what it is going to be loaded in destination register by LDR
                lastDataReadToR[ldrDestinationRegister] = value;

                // where it is going to be read from LDR
                lastDataAddressReadToR[ldrDestinationRegister] = ldrValueAddress;

                // position of the LDR instruction
                lastLDRInstructionAddress[ldrDestinationRegister] = instructionAddress;
            }
            // Check if it's a STR instruction
            else if (instructionParser.IsSTRInstruction(instruction))
            {
                var baseRegister = instructionParser.GetSTRBaseRegister(instruction);
                var sourceRegister = instructionParser.GetSTRSourceRegister(instruction);

                var okay = lastDataReadToR[baseRegister] == AGB_INTERRUPTION_HANDLER_LOCATION_ADDRESS
                           && instructionAddress - lastLDRInstructionAddress[sourceRegister] < 64 // last instruction that wrote on source reg is near
                           && instructionAddress - lastLDRInstructionAddress[baseRegister] < 64 // last instruction that wrote on base reg is near
                           && lastLDRInstructionAddress[sourceRegister] != 0 // we already found a ldr to source reg
                           && lastLDRInstructionAddress[baseRegister] != 0 // we already found a ldr to base reg
                           && (lastDataReadToR[sourceRegister] & 0xFF000000) == 0x03000000; // source register contains an address value which is in IWRAM

                if (!thumb)
                {
                    // if STR has been found directly in joybus entry point? 
                    if (!okay && lastDataReadToR[baseRegister] == AGB_INTERRUPTION_HANDLER_LOCATION_ADDRESS)
                        if (instructionAddress == 0xE0)
                            okay = true;

                    // if the STR instruction comes before last write to base or source reg,
                    // how this can happen if file is scanned sequentially?
                    if (instructionAddress < lastLDRInstructionAddress[sourceRegister] ||
                        instructionAddress < lastLDRInstructionAddress[baseRegister])
                        okay = false;
                }

                if (!okay) continue;

                var lastReadDataAddress = lastDataAddressReadToR[baseRegister];

                // Create activation patch information
                var activationPatchBinary = ActivationPatchData.GetBinaryPatchFor(
                    (uint) (AGB_ROM_BASE_ADDRESS + lastActivationPatchAddress),
                    AGB_ROM_BASE_ADDRESS + PatchAddress,
                    (uint) (AGB_ROM_BASE_ADDRESS + instructionAddress + (thumb ? 3 : ARM_WORD_SIZE)),
                    sourceRegister,
                    baseRegister
                );

                var activationPatchData = new ActivationPatchData((uint) lastActivationPatchAddress,
                    activationPatchBinary, baseRegister, instructionAddress, lastReadDataAddress, thumb);

                activationPatchesData.Add(activationPatchData);

                lastActivationPatchAddress += activationPatchBinary.Length;

                Console.WriteLine("Patched an interrupt installer!");
                Console.WriteLine(
                    $"@0x{lastLDRInstructionAddress[sourceRegister]:X8}: r{sourceRegister}=0x{lastDataReadToR[sourceRegister]:X8}");
                Console.WriteLine(
                    $"@0x{lastLDRInstructionAddress[baseRegister]:X8}: r{baseRegister}=0x{lastDataReadToR[baseRegister]:X8}");
                Console.WriteLine(
                    $"@0x{instructionAddress:X8}: str r{sourceRegister},[r{baseRegister}]");
            }
        }

        return activationPatchesData;
    }

    private int CalculatePatchedROMSize(List<ActivationPatchData> activationPatchesData)
    {
        var patchedROMSize = GetROMSize();

        patchedROMSize = (int) Math.Max(patchedROMSize, PatchAddress + SleepPatchBinary.Length);

        activationPatchesData.ForEach(activationPatchData =>
        {
            patchedROMSize = (int) Math.Max(patchedROMSize,
                activationPatchData.Address + activationPatchData.Patch.Length);
        });

        return patchedROMSize;
    }

    private bool IsValidROMAddress(uint address)
    {
        return address <= GetROMSize() - ARM_WORD_SIZE;
    }

    private bool IsAlignedAddress(uint address, bool thumb)
    {
        if (thumb)
            return (address & 0x03) == 0x00;

        return address % ARM_WORD_SIZE == 0x00;
    }

    public bool PatchIsValid()
    {
        return GetROMSize() % ARM_WORD_SIZE == 0;
    }
}