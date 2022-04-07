namespace GBA.Sleephack;

public class ARMThumbInstructionParser : IARMInstructionParser
{
    public bool IsLDRInstruction(uint instruction)
    {
        return (instruction & 0xF800) == 0x4800;
    }

    public uint GetLDRDestinationRegister(uint ldrInstruction)
    {
        return (ldrInstruction >> 8) & 0x07;
    }

    public uint GetLDRValueAddress(uint ldrInstruction, uint ldrInstructionAddress)
    {
        var programCounter = ldrInstructionAddress + 4;

        return programCounter + (ldrInstruction & 0xFF) * 4;
    }

    public bool IsSTRInstruction(uint instruction)
    {
        return (instruction & 0xFFC0) == 0x6000;
    }

    public uint GetSTRBaseRegister(uint strInstruction)
    {
        return (strInstruction >> 3) & 0x07;
    }

    public uint GetSTRSourceRegister(uint strInstruction)
    {
        return (strInstruction >> 0) & 0x07;
    }
}