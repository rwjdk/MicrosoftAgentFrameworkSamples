using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace AgentFramework.Toolkit.Agents.Models;

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
    public int? MaxOutputTokens { get; set; }
    public Action<ChatClientAgentOptions>? AdditionalChatClientAgentOptions { get; set; }

    public AIAgent ApplyMiddleware(AIAgent innerAgent)
    {
        //todo - more middleware options
        if (RawToolCallDetails != null)
        {
            innerAgent = innerAgent.AsBuilder().Use(new ToolCallsHandler(RawToolCallDetails).ToolCallingMiddleware).Build();
        }

        return innerAgent;
    }
}