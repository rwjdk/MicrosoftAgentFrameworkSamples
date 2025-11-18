using AgentFramework.Utilities.OpenAI;

namespace AgentFramework.Utilities.Grok;

public class GrokAgentFactory(GrokConnection connection)
{
    private readonly OpenAIAgentFactory _openAIAgentFactory = new(new OpenAIConnection
    {
        ApiKey = connection.ApiKey,
        AdditionalOpenAIClientOptions = connection.AdditionalOpenAIClientOptions,
        Endpoint = "https://api.x.ai/v1"
    });

    public Agent CreateAgent(OpenAIResponseWithoutReasoningOptions options)
    {
        Agent agent = _openAIAgentFactory.CreateAgent(options);
        agent.Provider = AgentProvider.XAiGrokResponses;
        return agent;
    }

    public Agent CreateAgent(OpenAIResponseWithReasoningOptions options)
    {
        Agent agent = _openAIAgentFactory.CreateAgent(options);
        agent.Provider = AgentProvider.XAiGrokResponses;
        return agent;
    }

    public Agent CreateAgent(OpenAIChatClientWithoutReasoningOptions options)
    {
        Agent agent = _openAIAgentFactory.CreateAgent(options);
        agent.Provider = AgentProvider.XAiGrokChatClient;
        return agent;
    }

    public Agent CreateAgent(OpenAIChatClientWithReasoningOptions options)
    {
        Agent agent = _openAIAgentFactory.CreateAgent(options);
        agent.Provider = AgentProvider.XAiGrokChatClient;
        return agent;
    }
}