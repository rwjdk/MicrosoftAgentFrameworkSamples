using Microsoft.Extensions.AI;

namespace MicrosoftAgentFramework.Utilities.Extensions;

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
    }
}