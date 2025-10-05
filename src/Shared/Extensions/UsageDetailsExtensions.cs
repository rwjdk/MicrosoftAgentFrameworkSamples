using Microsoft.Extensions.AI;

namespace Shared.Extensions;

public static class UsageDetailsExtensions
{
    private const string ReasonTokenCountKey = "OutputTokenDetails.ReasoningTokenCount";

    public static void OutputAsInformation(this UsageDetails? usageDetails)
    {
        if (usageDetails == null)
        {
            return;
        }

        Utils.Separator();
        Utils.WriteLineInformation($"- Input Tokens: {usageDetails.InputTokenCount}");
        Utils.WriteLineInformation($"- Output Tokens: {usageDetails.OutputTokenCount} " +
                                   $"({usageDetails.GetOutputTokensUsedForReasoning()} was used for reasoning)");
    }

    public static long? GetOutputTokensUsedForReasoning(this UsageDetails? usageDetails)
    {
        if (usageDetails?.AdditionalCounts?.TryGetValue(ReasonTokenCountKey, out long tokenCount) ?? false)
        {
            return tokenCount;
        }

        return null;
    }
}