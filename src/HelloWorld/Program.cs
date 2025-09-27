using HelloWorld.Models;
using Shared;

Configuration configuration = ConfigurationManager.GetConfiguration();

Scenario scenario = Scenario.MultiAgentOrchestration;

switch (scenario)
{
    case Scenario.MultiAgentOrchestration:
    {
        break;
    }
    case Scenario.WorkflowOrchestration:
    {
        break;
    }
    case Scenario.ExternalAgentFramework:
    {
        AgentType agentType = AgentType.ChatCompletionAgent;
        switch (agentType)
        {
            case AgentType.ChatCompletionAgent:
            {
                break;
            }
            case AgentType.AzureAiFoundryAgent:
            {
                break;
            }
            case AgentType.OpenAiAssistantAgent:
            {
                break;
            }
            case AgentType.OpenAiResponsesAgent:
            {
                break;
            }
            default:
                throw new ArgumentOutOfRangeException();
        }

        break;
    }
    default:
        throw new ArgumentOutOfRangeException();
}