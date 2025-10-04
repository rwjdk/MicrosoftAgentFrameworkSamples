namespace ToolCalling.Advanced.Tools;

public class DangerousTools
{
    public static void SomethingDangerous(int value = 42)
    {
        Console.WriteLine($"Did something dangerous with the value: {value}");
    }
}