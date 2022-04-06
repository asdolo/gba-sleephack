namespace GBA.Sleephack;

public static class Constants
{
    public const int ARM_WORD_SIZE = 4;
    public const int ARM_HALF_WORD_SIZE = 2;
    public const uint AGB_ROM_BASE_ADDRESS = 0x08000000;
    public const uint AGB_ROM_MAX_SIZE = 0x2000000; // 32 Mebibyte
    
    [Flags]
    public enum Buttons : uint
    {
        A       = 1 << 0,
        B       = 1 << 1,
        SELECT  = 1 << 2,
        START   = 1 << 3,
        RIGHT   = 1 << 4,
        LEFT    = 1 << 5,
        UP      = 1 << 6,
        DOWN    = 1 << 7,
        R       = 1 << 8,
        L       = 1 << 9,
    };
}