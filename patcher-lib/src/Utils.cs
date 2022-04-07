namespace GBA.Sleephack;

public static class Utils
{
    public static int RoundUp(int numToRound, int multiple)
    {
        if (multiple == 0)
            return numToRound;

        var remainder = numToRound % multiple;
        if (remainder == 0)
            return numToRound;

        return numToRound + multiple - remainder;
    }
}