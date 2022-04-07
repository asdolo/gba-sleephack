namespace GBA.Sleephack;

public class ARMInstructionParser : IARMInstructionParser
{
    private const int UP_DOWN_BIT_MASK = 0x800000;

    public bool IsLDRInstruction(uint instruction)
    {
        return (instruction & 0xFE7F0000) == 0xE41F0000;
    }

    public uint GetLDRDestinationRegister(uint ldrInstruction)
    {
        return (ldrInstruction >> 12) & 0x0F;
    }

    public uint GetLDRValueAddress(uint ldrInstruction, uint ldrInstructionAddress)
    {
        // real PC, takes care of ARM7 pipeline so adds 8 (https://stackoverflow.com/a/24116090)
        var programCounter = ldrInstructionAddress + 8;

        return (uint) (programCounter + GetLDRUpDown(ldrInstruction) * (ldrInstruction & 0xFFF));
    }

    private int GetLDRUpDown(uint ldrInstruction)
    {
        // if u bit is set we're adding to base offset, otherwise subtracting
        return (ldrInstruction & UP_DOWN_BIT_MASK) == UP_DOWN_BIT_MASK ? 1 : -1;
    }

    public bool IsSTRInstruction(uint instruction)
    {
        return (instruction & 0xFE700FFF) == 0xE4000000;
    }

    public uint GetSTRBaseRegister(uint strInstruction)
    {
        return (strInstruction >> 16) & 0x0F;
    }

    public uint GetSTRSourceRegister(uint strInstruction)
    {
        return (strInstruction >> 12) & 0x0F;
    }
}