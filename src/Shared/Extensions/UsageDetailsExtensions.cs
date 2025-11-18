using Microsoft.Extensions.AI;

namespace Shared.Extensions;

public static class UsageDetailsExtensions
{
    private const string ReasonTokenCountKey = "OutputTokenDetails.ReasoningTokenCount";

    extension(UsageDetails? usageDetails)
    {
        public long? GetOutputTokensUsedForReasoning()
        {
            if (usageDetails?.AdditionalCounts?.TryGetValue(ReasonTokenCountKey, out long tokenCount) ?? false)
            {
                return tokenCount;
            }

            return null;
        }

        public void OutputAsInformation()
        {
            if (usageDetails == null)
            {
                return;
            }

            Utils.Separator();
            Utils.WriteLineDarkGray($"- Input Tokens: {usageDetails.InputTokenCount}");
            Utils.WriteLineDarkGray($"- Output Tokens: {usageDetails.OutputTokenCount} " +
                                    $"({usageDetails.GetOutputTokensUsedForReasoning()} was used for reasoning)");
        }
    }
}