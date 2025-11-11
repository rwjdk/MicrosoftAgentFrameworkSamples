using Microsoft.Extensions.AI;
using System.Text;

namespace AgentFramework.Toolkit.AIAgents.Models;

public class ToolCallingDetails
{
    public required FunctionInvocationContext Context { get; set; }
    //Todo - more easy info

    public override string ToString()
    {
        StringBuilder toolDetails = new();
        toolDetails.Append($"- Tool Call: '{Context.Function.Name}'");
        if (Context.Arguments.Count > 0)
        {
            toolDetails.Append($" (Args: {string.Join(",", Context.Arguments.Select(x => $"[{x.Key} = {x.Value}]"))}");
        }

        return toolDetails.ToString();
    }
}