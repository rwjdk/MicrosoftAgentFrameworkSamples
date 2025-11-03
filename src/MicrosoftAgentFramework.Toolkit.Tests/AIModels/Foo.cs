using Microsoft.Agents.AI;
using MicrosoftAgentFramework.Toolkit.AIAgents;
using MicrosoftAgentFramework.Toolkit.AIModels;
using Shared;
using GenerativeAI;

namespace MicrosoftAgentFramework.Toolkit.Tests.AIModels;

public class Foo
{
    [Fact]
    public async Task AzureOpenAi()
    {
        Configuration configuration = ConfigurationManager.GetConfiguration(); //todo - should not be dependent on this!

        AzureOpenAIAgentFactory factory = new(configuration.AzureOpenAiEndpoint, configuration.AzureOpenAiKey);

        Agent agent = factory.GetAgent(AzureOpenAIModel.ChatClientReasoningModel("gpt-4.1", "low", TimeSpan.FromMinutes(5)));
        AgentRunResponse response = await agent.RunAsync("Hello");
    }

    [Fact]
    public async Task Google()
    {
        Configuration configuration = ConfigurationManager.GetConfiguration(); //todo - should not be dependent on this!

        GoogleAgentFactory factory = new(configuration.GoogleGeminiApiKey);
        Agent agent = factory.GetAgent(new GoogleModel(GoogleAIModels.Gemini25Flash));
        AgentRunResponse response = await agent.RunAsync("Hello");
    }
}