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

            Utils.Gray($"- Input Tokens: {usageDetails.InputTokenCount}");
            var output = $"- Output Tokens: {usageDetails.OutputTokenCount} " + $"({usageDetails.ReasoningTokenCount ?? 0} was used for reasoning)";
            Utils.Gray(output);
        }
    }
}