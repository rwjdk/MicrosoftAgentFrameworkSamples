using JetBrains.Annotations;
using Microsoft.Extensions.AI;
using MicrosoftAgentFramework.Utilities.Extensions;

namespace Shared.Extensions;

[PublicAPI]
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
        Utils.WriteLineDarkGray($"- Input Tokens: {usageDetails.InputTokenCount}");
        Utils.WriteLineDarkGray($"- Output Tokens: {usageDetails.OutputTokenCount} " +
                                $"({usageDetails.GetOutputTokensUsedForReasoning()} was used for reasoning)");
    }
}