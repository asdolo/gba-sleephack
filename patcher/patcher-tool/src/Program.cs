using GBA.Sleephack;

public static class Program
{
    public static int Main(string[] args)
    {
        var argCount = args.Length;

        if (argCount != 2)
        {
            Console.WriteLine("syntax: <input file path> <output file path>");
            return 1;
        }

        var inputFilePath = args[0];
        var outputFilePath = args[1];

        var romBinary = File.ReadAllBytes(inputFilePath);

        var patcher = new Patcher(romBinary);

        if (!patcher.PatchIsValid()) throw new ArgumentException("Patch file is not valid!");

        var patchedROM = patcher.Patch();

        File.WriteAllBytes(outputFilePath, patchedROM);

        return 0;
    }
}