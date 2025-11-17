using Microsoft.Extensions.AI;

namespace AgentFramework.Toolkit.OpenAI.Usage;

public static class UsageExtensions
{
    extension(UsageDetails? usageDetails)
    {
        /// <summary>
        /// Gets the number of tokens used for reasoning that was part of the total OutputTokenCount.
        /// </summary>
        public long OutputReasoningTokenCount
        {
            get
            {
                if (usageDetails?.AdditionalCounts?.TryGetValue("OutputTokenDetails.ReasoningTokenCount", out long tokenCount) ?? false)
                {
                    return tokenCount;
                }

                return 0;
            }
        }
    }
    
    extension(UsageDetails? usageDetails)
    {
        /// <summary>
        /// Reused tokens in conversation history (often billed at a reduced rate).
        /// </summary>
        public long InputCachedTokenCount {
            get
            {
                if (usageDetails?.AdditionalCounts?.TryGetValue("InputTokenDetails.CachedTokenCount", out long tokenCount) ?? false)
                {
                    return tokenCount;
                }

                return 0;
            }
        }
    }
}