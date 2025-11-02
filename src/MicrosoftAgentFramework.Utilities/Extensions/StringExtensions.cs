using System.Text.RegularExpressions;

namespace MicrosoftAgentFramework.Utilities.Extensions;

internal static class StringExtensions
{
    internal static string ToSnakeCase(this string input)
    {
        var result = Regex.Replace(input, "([a-z0-9])([A-Z])", "$1_$2");
        return result.ToLower();
    }
}