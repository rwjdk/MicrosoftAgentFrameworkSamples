using System.Text;

namespace DoItYourselfAiCalling.Helpers;

public static class ConsoleHelper
{
    public static string ReadString(string prompt)
    {
        Console.Write(prompt+": ");
        return Console.ReadLine() ?? string.Empty;
    }

    public static string ReadSecret(string prompt)
    {
        Console.Write(prompt + ": ");

        StringBuilder secret = new();

        while (true)
        {
            ConsoleKeyInfo key = Console.ReadKey(intercept: true);

            switch (key.Key)
            {
                case ConsoleKey.Enter:
                    Console.WriteLine();
                    return secret.ToString();
                case ConsoleKey.Backspace:
                {
                    if (secret.Length > 0)
                        secret.Length--;

                    continue;
                }
            }

            if (!char.IsControl(key.KeyChar))
                secret.Append(key.KeyChar);
        }
    }
}