using Microsoft.Extensions.AI;

namespace Playground.Extensions;

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
        public long InputCachedTokenCount
        {
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

    extension(UsageDetails? usageDetails)
    {
        public long CacheCreationInputTokenCount
        {
            get
            {
                if (usageDetails?.AdditionalCounts?.TryGetValue("CacheCreationInputTokens", out long tokenCount) ?? false)
                {
                    return tokenCount;
                }

                return 0;
            }
        }
    }

    extension(UsageDetails? usageDetails)
    {
        public long CacheReadInputTokenCount
        {
            get
            {
                if (usageDetails?.AdditionalCounts?.TryGetValue("CacheReadInputTokens", out long tokenCount) ?? false)
                {
                    return tokenCount;
                }

                return 0;
            }
        }
    }
}