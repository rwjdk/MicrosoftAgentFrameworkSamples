namespace Shared;

public static class Utils
{
    public static void WriteLineError(Exception e)
    {
        WriteLineError(e.ToString());
    }

    public static void WriteLineError(string text)
    {
        WriteLine(text, ConsoleColor.Red);
    }

    public static void WriteLineYellow(string text)
    {
        WriteLine(text, ConsoleColor.Yellow);
    }

    public static void WriteLineWarning(string text)
    {
        WriteLine(text, ConsoleColor.Yellow);
    }

    public static void WriteLineInformation(string text)
    {
        WriteLine(text, ConsoleColor.DarkGray);
    }

    public static void WriteLineSuccess(string text)
    {
        WriteLine(text, ConsoleColor.Green);
    }

    public static void WriteLine(string text, ConsoleColor color)
    {
        ConsoleColor orgColor = Console.ForegroundColor;
        try
        {
            Console.ForegroundColor = color;
            Console.WriteLine(text);
        }
        finally
        {
            Console.ForegroundColor = orgColor;
        }
    }

    public static void Separator()
    {
        Console.WriteLine();
        WriteLine("".PadLeft(Console.WindowWidth, '-'), ConsoleColor.Gray);
        Console.WriteLine();
    }
}