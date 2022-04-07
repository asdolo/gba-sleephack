namespace GBA.Sleephack;

public interface IARMInstructionParser
{
    bool IsLDRInstruction(uint instruction);
    uint GetLDRDestinationRegister(uint ldrInstruction);
    uint GetLDRValueAddress(uint ldrInstruction, uint instructionAddress);
    bool IsSTRInstruction(uint instruction);
    uint GetSTRBaseRegister(uint strInstruction);
    uint GetSTRSourceRegister(uint strInstruction);
}