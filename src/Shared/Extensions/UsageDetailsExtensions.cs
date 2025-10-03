﻿using Microsoft.Extensions.AI;

namespace Shared.Extensions;

public static class UsageDetailsExtensions
{
    private const string ReasonTokenCountKey = "OutputTokenDetails.ReasoningTokenCount";

    public static long? GetOutputTokensUsedForReasoning(this UsageDetails? usageDetails)
    {
        if (usageDetails?.AdditionalCounts?.TryGetValue(ReasonTokenCountKey, out long tokenCount) ?? false)
        {
            return tokenCount;
        }

        return null;
    }
}