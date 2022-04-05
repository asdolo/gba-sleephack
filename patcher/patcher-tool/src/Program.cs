using GBA.Sleephack;

public static class Program
{
    public static int Main(string[] args)
    {
        var argCount = args.Length;

        if (argCount != 5)
        {
            Console.WriteLine("syntax: <input file path> <output file path> <sleep button combination> <wake button combination> <hard reset button combination>");
            Console.WriteLine("example: gba-sleephack-patcher-tool.exe input.gba output.gba \"L+R+Select\" \"Select+Start\" \"L+R+Select+Start\"");
            return 1;
        }

        var inputFilePath = args[0];
        var outputFilePath = args[1];

        var romBinary = File.ReadAllBytes(inputFilePath);

        var buttonsArgParser = new ButtonsParser();

        var sleepButtonCombination = buttonsArgParser.Parse(args[2]);
        var wakeUpButtonCombination = buttonsArgParser.Parse(args[3]);
        var hardResetButtonCombination = buttonsArgParser.Parse(args[4]);

        var patcher = new Patcher(romBinary, sleepButtonCombination, wakeUpButtonCombination, hardResetButtonCombination);

        if (!patcher.PatchIsValid()) throw new ArgumentException("Patch file is not valid!");

        var patchedROM = patcher.Patch();

        File.WriteAllBytes(outputFilePath, patchedROM);

        return 0;
    }
}