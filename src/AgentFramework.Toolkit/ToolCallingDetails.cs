using Microsoft.Extensions.AI;

namespace AgentFramework.Toolkit;

public class ToolCallingDetails
{
    public required FunctionInvocationContext Context { get; set; }
    //Todo - more easy info
}