using GBA.Sleephack;

public static class Program
{
    public static int Main(string[] args)
    {
        var argCount = args.Length;

        if (argCount != 3)
        {
            Console.WriteLine("syntax: <input file path> <output file path> <patch address>");
            return 1;
        }

        var inputFilePath = args[0];
        var outputFilePath = args[1];
        var patchAddress = Convert.ToUInt32(args[2]);

        var rom = File.ReadAllBytes(inputFilePath);
        var patch = File.ReadAllBytes("patch.bin");

        var patcher = new Patcher(rom, patch, patchAddress);

        if (!patcher.PatchIsValid()) throw new ArgumentException("Patch file is not valid!");

        var patchedROM = patcher.Patch();

        File.WriteAllBytes(outputFilePath, patchedROM);

        return 0;
    }
}