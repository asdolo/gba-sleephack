using GBA.Sleephack;

public class ButtonsParser
{
    public Constants.Buttons Parse(string buttonCombo)
    {
        try
        {
            return (Constants.Buttons) Enum.Parse(
                typeof(Constants.Buttons),
                string.Join(
                    ", ",
                    buttonCombo
                        .Split("+")
                        .Select(button => button.Trim().ToUpper())
                )
            );
        }
        catch (ArgumentException)
        {
            throw new ArgumentException($"Invalid button combo: \"{buttonCombo}\"");
        }
    }
}