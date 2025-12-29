using Microsoft.Extensions.AI;

namespace Shared.Extensions;

public static class UsageDetailsExtensions
{
    extension(UsageDetails? usageDetails)
    {
        public void OutputAsInformation()
        {
            if (usageDetails == null)
            {
                return;
            }

            Utils.WriteLineDarkGray($"- Input Tokens: {usageDetails.InputTokenCount}");
            Utils.WriteLineDarkGray($"- Output Tokens: {usageDetails.OutputTokenCount} " +
                                    $"({usageDetails.ReasoningTokenCount} was used for reasoning)");
        }
    }
}