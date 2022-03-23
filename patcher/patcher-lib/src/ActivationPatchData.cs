using System.Buffers.Binary;
using static GBA.Sleephack.Constants;

namespace GBA.Sleephack;

public class ActivationPatchData
{
    private const uint BX_INSTRUCTION_OPCODE = 0xE12FFF10;
    private const uint BX_INSTRUCTION_OPCODE_THUMB = 0x4700;
    private readonly uint _baseRegister;
    private readonly uint _ldrDataAddress;
    private readonly uint _strInstructionAddress;
    private readonly bool _thumb;

    public ActivationPatchData(uint address, byte[] patch, uint baseRegister, uint strInstructionAddress,
        uint ldrDataAddress, bool thumb)
    {
        Address = address;
        Patch = patch;

        _baseRegister = baseRegister;
        _strInstructionAddress = strInstructionAddress;
        _ldrDataAddress = ldrDataAddress;
        _thumb = thumb;
    }

    public uint Address { get; }
    public byte[] Patch { get; }


    public void ApplyTo(byte[] rom)
    {
        // Patch both STR and LDR instructions that we found
        PatchSTRInstructionIn(rom);
        PatchLDRInstructionIn(rom);

        // Copy activation routine 
        Array.Copy(Patch, 0, rom, Address, Patch.Length);
    }

    private void PatchSTRInstructionIn(byte[] rom)
    {
        var romSpan = rom.AsSpan();

        if (_thumb)
            BinaryPrimitives.WriteUInt16LittleEndian(romSpan.Slice((int) _strInstructionAddress, ARM_HALF_WORD_SIZE),
                (ushort) (BX_INSTRUCTION_OPCODE_THUMB + (_baseRegister << 3)));
        else
            BinaryPrimitives.WriteUInt32LittleEndian(romSpan.Slice((int) _strInstructionAddress, ARM_WORD_SIZE),
                BX_INSTRUCTION_OPCODE + _baseRegister);
    }

    private void PatchLDRInstructionIn(byte[] rom)
    {
        var romSpan = rom.AsSpan();

        var patchedAddressData = AGB_ROM_BASE_ADDRESS + Address;

        if (_ldrDataAddress <= rom.Length - ARM_WORD_SIZE)
            BinaryPrimitives.WriteInt32LittleEndian(
                romSpan.Slice((int) _ldrDataAddress, ARM_WORD_SIZE),
                (int) patchedAddressData
            );
    }

    public static byte[] GetBinaryPatchFor(uint activationAddress, uint routineAddress, uint returnAddress, uint rA,
        uint rB)
    {
        var activationRoutine = new List<uint>
        {
            0xE92D543F,
            0xE1A0C000 + rA,
            0xE1A01000 + rB,
            0xE1A0000C,
            0xEB000000 + (0x00FFFFFF & ((routineAddress - (activationAddress + 8 + 16)) >> 2)),
            0xE8BD543F,
            0xE59F0000 + (rB << 12),
            0xE12FFF10 + rB,
            returnAddress
        };

        return activationRoutine.SelectMany(instruction =>
        {
            var instructionBytes = new byte[ARM_WORD_SIZE];
            BinaryPrimitives.WriteUInt32LittleEndian(instructionBytes, instruction);

            return instructionBytes;
        }).ToArray();
    }
}