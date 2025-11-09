using Microsoft.Extensions.AI;

namespace AgentFramework.Toolkit.AIAgents.Models;

public class ToolCallingDetails
{
    public required FunctionInvocationContext Context { get; set; }
    //Todo - more easy info
}