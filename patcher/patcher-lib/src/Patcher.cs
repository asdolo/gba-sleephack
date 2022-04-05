using System.Buffers.Binary;
using static GBA.Sleephack.Constants;

namespace GBA.Sleephack;

public class Patcher
{
    private readonly byte[] _romBinary;
    private readonly byte[] _sleepPatchBinary = {
        0x01, 0x13, 0xA0, 0xE3, 0x4C, 0x00, 0x01, 0xE5, 0x34, 0x00, 0x8F, 0xE2, 0x48, 0x00, 0x01, 0xE5,
        0x6C, 0x01, 0x9F, 0xE5, 0x60, 0x00, 0x01, 0xE5, 0x68, 0x01, 0x9F, 0xE5, 0x5C, 0x00, 0x01, 0xE5,
        0x64, 0x01, 0x9F, 0xE5, 0x58, 0x00, 0x01, 0xE5, 0x60, 0x01, 0x9F, 0xE5, 0x54, 0x00, 0x01, 0xE5,
        0x5C, 0x01, 0x9F, 0xE5, 0x50, 0x00, 0x01, 0xE5, 0x58, 0x01, 0x9F, 0xE5, 0x04, 0x00, 0x01, 0xE5,
        0x1E, 0xFF, 0x2F, 0xE1, 0x30, 0x21, 0x90, 0xE5, 0x18, 0x00, 0x2D, 0xE9, 0x48, 0x31, 0x9F, 0xE5,
        0x48, 0x41, 0x9F, 0xE5, 0x04, 0x30, 0x23, 0xE0, 0x03, 0x00, 0x52, 0xE1, 0x18, 0x00, 0xBD, 0xE8,
        0x08, 0x00, 0x00, 0x0A, 0x18, 0x00, 0x2D, 0xE9, 0x34, 0x31, 0x9F, 0xE5, 0x2C, 0x41, 0x9F, 0xE5,
        0x04, 0x30, 0x23, 0xE0, 0x03, 0x00, 0x52, 0xE1, 0x18, 0x00, 0xBD, 0xE8, 0x00, 0x00, 0x00, 0x0A,
        0x4C, 0xF0, 0x10, 0xE5, 0x00, 0x00, 0x26, 0xEF, 0xF0, 0x4F, 0x2D, 0xE9, 0x60, 0x10, 0x80, 0xE2,
        0xFC, 0x03, 0xB1, 0xE8, 0xFC, 0x03, 0x2D, 0xE9, 0xFC, 0x03, 0xB1, 0xE8, 0xFC, 0x03, 0x2D, 0xE9,
        0x02, 0x1C, 0x80, 0xE2, 0xB0, 0x40, 0xD1, 0xE1, 0x30, 0x51, 0x90, 0xE5, 0xB0, 0x60, 0xD0, 0xE1,
        0xF0, 0x10, 0x9F, 0xE5, 0x00, 0x12, 0x80, 0xE5, 0x03, 0x11, 0xA0, 0xE3, 0x04, 0x30, 0x2D, 0xE5,
        0xE4, 0x30, 0x9F, 0xE5, 0x03, 0x38, 0xA0, 0xE1, 0x03, 0x10, 0x81, 0xE1, 0x04, 0x30, 0x9D, 0xE4,
        0x30, 0x11, 0x80, 0xE5, 0xB4, 0x08, 0xC0, 0xE1, 0x80, 0x10, 0x86, 0xE3, 0xB0, 0x10, 0xC0, 0xE1,
        0x00, 0x00, 0x03, 0xEF, 0x04, 0x30, 0x2D, 0xE5, 0xBC, 0x30, 0x9F, 0xE5, 0x01, 0x03, 0xA0, 0xE3,
        0x30, 0x11, 0x90, 0xE5, 0x03, 0x00, 0x11, 0xE1, 0xFB, 0xFF, 0xFF, 0x1A, 0xB6, 0x10, 0xD0, 0xE1,
        0x9F, 0x00, 0x51, 0xE3, 0xFC, 0xFF, 0xFF, 0x1A, 0xB6, 0x10, 0xD0, 0xE1, 0xA0, 0x00, 0x51, 0xE3,
        0xFC, 0xFF, 0xFF, 0x1A, 0xB6, 0x10, 0xD0, 0xE1, 0x9F, 0x00, 0x51, 0xE3, 0xFC, 0xFF, 0xFF, 0x1A,
        0xB6, 0x10, 0xD0, 0xE1, 0xA0, 0x00, 0x51, 0xE3, 0xFC, 0xFF, 0xFF, 0x1A, 0xB6, 0x10, 0xD0, 0xE1,
        0x9F, 0x00, 0x51, 0xE3, 0xFC, 0xFF, 0xFF, 0x1A, 0x04, 0x30, 0x9D, 0xE4, 0x02, 0x1C, 0x80, 0xE2,
        0xB0, 0x40, 0xC1, 0xE1, 0x30, 0x51, 0x80, 0xE5, 0x01, 0x4A, 0xA0, 0xE3, 0xB2, 0x40, 0xC1, 0xE1,
        0xB0, 0x60, 0xC0, 0xE1, 0xFC, 0x03, 0xBD, 0xE8, 0x84, 0x30, 0x80, 0xE5, 0x80, 0x10, 0x80, 0xE2,
        0xFC, 0x03, 0xA1, 0xE8, 0x60, 0x10, 0x80, 0xE2, 0xFC, 0x03, 0xBD, 0xE8, 0xFC, 0x03, 0xA1, 0xE8,
        0xF0, 0x4F, 0xBD, 0xE8, 0xB6, 0x10, 0xD0, 0xE1, 0xA0, 0x00, 0x51, 0xE3, 0xFC, 0xFF, 0xFF, 0x1A,
        0x4C, 0xF0, 0x10, 0xE5, 0x00, 0x12, 0x90, 0xE5, 0x01, 0x08, 0x11, 0xE3, 0x01, 0x02, 0x11, 0x03,
        0x4C, 0xF0, 0x10, 0x05, 0x48, 0xF0, 0x10, 0xE5, 0xA0, 0x7F, 0x00, 0x03, 0xAA, 0x02, 0x00, 0x00,
        0xFF, 0x03, 0x00, 0x00, 0xEB, 0x02, 0x00, 0x00, 0x00, 0x30, 0xFF, 0xFF, 0xAB, 0x03, 0x00, 0x00
    };
    private uint _patchAddress;
    private readonly Buttons _sleepButtonCombination;
    private readonly Buttons _wakeUpButtonCombination;
    private readonly Buttons _hardResetButtonCombination;

    private const int ARM_REGISTERS_COUNT = 16;
    private const uint DUMMY_DATA = 0xDEADBEEF;

    private const uint AGB_INTERRUPTION_HANDLER_LOCATION_ADDRESS = 0x03007FFC;
    private const uint PATCH_SLEEP_BUTTON_COMBO_OFFSET = 0x19C;
    private const uint PATCH_WAKE_UP_BUTTON_COMBO_OFFSET = 0x1AC;
    private const uint PATCH_HARD_RESET_BUTTON_COMBO_OFFSET = 0x1A4;

    public Patcher(byte[] romBinary, Buttons sleepButtonCombination,
        Buttons wakeUpButtonCombination, Buttons hardResetButtonCombination)
    {
        _romBinary = romBinary;
        _sleepButtonCombination = sleepButtonCombination;
        _wakeUpButtonCombination = wakeUpButtonCombination;
        _hardResetButtonCombination = hardResetButtonCombination;

        // We start with a dummy patch address. We'll find a real one later...
        _patchAddress = DUMMY_DATA;
    }

    private int GetROMSize()
    {
        return _romBinary.Length;
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

        _patchAddress = (uint) address;

        var romBytesSpan = _romBinary.AsSpan();

        // Add first Activation Patch just after the Sleep Patch
        var lastActivationPatchAddress = _patchAddress + _sleepPatchBinary.Length;

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
        Array.Copy(_romBinary, 0, patchedROM, 0, GetROMSize());
        
        // Change button combinations for sleep, wake up and hard reset

        BinaryPrimitives.WriteUInt32LittleEndian(
            _sleepPatchBinary.AsSpan()[(int) PATCH_SLEEP_BUTTON_COMBO_OFFSET..],
            (uint) _sleepButtonCombination
        );
        BinaryPrimitives.WriteUInt32LittleEndian(
            _sleepPatchBinary.AsSpan()[(int) PATCH_WAKE_UP_BUTTON_COMBO_OFFSET..],
            (uint) _wakeUpButtonCombination
        );
        BinaryPrimitives.WriteUInt32LittleEndian(
            _sleepPatchBinary.AsSpan()[(int) PATCH_HARD_RESET_BUTTON_COMBO_OFFSET..],
            (uint) _hardResetButtonCombination
        );

        // Copy sleep patch to the indicated address in the patched ROM
        Array.Copy(_sleepPatchBinary, 0, patchedROM, _patchAddress, _sleepPatchBinary.Length);

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
            GetActivationPatchesData(GetROMWordCount(), _romBinary.AsSpan(), ref dummyAddress, false)
                .Select(patchData => patchData.Patch.Length)
                .Sum() +
            GetActivationPatchesData(GetROMHalfWordCount(), _romBinary.AsSpan(), ref dummyAddress, true)
                .Select(patchData => patchData.Patch.Length)
                .Sum();

        var totalPatchesSize = _sleepPatchBinary.Length + activationPatchesSize;

        // Expand to next multiple of four words.
        totalPatchesSize = Utils.RoundUp(totalPatchesSize, 4 * ARM_WORD_SIZE);

        // With the total size of all the patches we can now scan the ROM for a suitable location to fit the whole thing
        try
        {
            return EmptyDataAddressFinder.For(_romBinary).FindWithSize(totalPatchesSize);
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
                    AGB_ROM_BASE_ADDRESS + _patchAddress,
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

        patchedROMSize = (int) Math.Max(patchedROMSize, _patchAddress + _sleepPatchBinary.Length);

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