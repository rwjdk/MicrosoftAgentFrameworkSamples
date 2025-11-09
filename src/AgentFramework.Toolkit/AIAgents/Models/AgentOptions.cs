using Microsoft.Extensions.AI;

namespace AgentFramework.Toolkit.AIAgents.Models;

public abstract class AgentOptions
{
    public required string DeploymentModelName { get; set; }
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Instructions { get; set; }
    public IList<AITool>? Tools { get; set; }
    public TimeSpan? NetworkTimeout { get; set; }
    public Action<RawCallDetails>? RawHttpCallDetails { get; set; }
    public Action<ToolCallingDetails>? RawToolCallDetails { get; set; }
}