namespace DoItYourselfAiCalling.Helpers;

public static class EnvironmentVariableHelper
{
    public static string GetValueOrAsk(string variableName, bool secret = false)
    {
        string? variableValue = Environment.GetEnvironmentVariable(variableName, EnvironmentVariableTarget.User);
        if (!string.IsNullOrWhiteSpace(variableValue))
        {
            return variableValue;
        }

        //No value; Ask for it
        string newValue = secret ? ConsoleHelper.ReadSecret(variableName) : ConsoleHelper.ReadString(variableName);
        Environment.SetEnvironmentVariable(variableName, newValue, EnvironmentVariableTarget.User);
        return newValue;

    }
}