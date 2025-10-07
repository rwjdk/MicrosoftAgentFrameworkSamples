namespace MultiAgent.AgentAsTool;

public static class NumberTools
{
    public static int AnswerToEverythingNumber()
    {
        return 42;
    }

    public static int RandomNumber(int min = 0, int max = 100)
    {
        return Random.Shared.Next(min, max + 1);
    }
}