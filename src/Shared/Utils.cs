using JetBrains.Annotations;
using System.Text;

namespace Shared;

[PublicAPI]
public static class Utils
{
    public static void Red(Exception e)
    {
        Red(e.ToString());
    }

    public static void Red(string text)
    {
        WriteLine(text, ConsoleColor.Red);
    }

    public static void Yellow(string text)
    {
        WriteLine(text, ConsoleColor.Yellow);
    }

    public static void Gray(string text)
    {
        WriteLine(text, ConsoleColor.DarkGray);
    }

    public static void Green(string text)
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

    public static void Init(string? title = null)
    {
        Console.Clear();
        Console.OutputEncoding = Encoding.UTF8;
        if (!string.IsNullOrWhiteSpace(title))
        {
            Gray($"--- {title} ---");
        }
    }
}