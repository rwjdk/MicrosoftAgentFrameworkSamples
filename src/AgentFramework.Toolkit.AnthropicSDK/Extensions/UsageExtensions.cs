using Microsoft.Extensions.AI;

namespace AgentFramework.Toolkit.AnthropicSDK.Extensions;

public static class UsageExtensions
{
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