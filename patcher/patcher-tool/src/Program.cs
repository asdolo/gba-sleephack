using GBA.Sleephack;
using CliFx;
using CliFx.Attributes;
using CliFx.Exceptions;
using CliFx.Infrastructure;

public static class Program
{
    [Command]
    public class MainCommand : ICommand
    {
        [CommandParameter(0, Name = "Input file path")]
        public string InputFilePath { get; init; }
        
        [CommandParameter(1, Name = "Output file path")]
        public string OutputFilePath { get; init; }

        [CommandOption("sleep-combo", Description = "Sleep buttons combination")]
        public string SleepButtonCombination { get; init; } = "L+R+Select";

        [CommandOption("wake-up-combo", Description = "Wake up buttons combination")]
        public string WakeUpButtonCombination { get; init; } = "Select+Start";
        
        [CommandOption("hard-reset-combo", Description = "Hard reset buttons combination")]
        public string HardResetButtonCombination { get; init; } = "L+R+Select+Start";
        
        public ValueTask ExecuteAsync(IConsole console)
        {
            var romBinary = File.ReadAllBytes(InputFilePath);

            var buttonsArgParser = new ButtonsParser();

            try
            {
                var sleepButtonCombination = buttonsArgParser.Parse(SleepButtonCombination);
                var wakeUpButtonCombination = buttonsArgParser.Parse(WakeUpButtonCombination);
                var hardResetButtonCombination = buttonsArgParser.Parse(HardResetButtonCombination);
                
                var patcher = new Patcher(romBinary, sleepButtonCombination, wakeUpButtonCombination, hardResetButtonCombination);

                var patchedROM = patcher.GetPatchedROM();
                
                File.WriteAllBytes(OutputFilePath, patchedROM);
            }
            catch (Exception ex)
            {
                throw new CommandException(ex.Message, 1);
            }

            console.Output.WriteLine($"Sleep button combination: {SleepButtonCombination}");
            console.Output.WriteLine($"Wake up button combination: {WakeUpButtonCombination}");
            console.Output.WriteLine($"Hard reset button combination: {HardResetButtonCombination}");
            console.Output.WriteLine();

            console.ForegroundColor = ConsoleColor.Green;
            console.Output.WriteLine($"Done! Patched ROM saved as {OutputFilePath}");
            console.ResetColor();

            return ValueTask.CompletedTask;
        }
    }

    public static async Task<int> Main() =>
        await new CliApplicationBuilder()
            .AddCommandsFromThisAssembly()
            .Build()
            .RunAsync();
}