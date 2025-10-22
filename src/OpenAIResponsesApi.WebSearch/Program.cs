using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI;
using Shared;
using Shared.Extensions;

Console.Clear();
Configuration configuration = ConfigurationManager.GetConfiguration();
OpenAIClient client = new(configuration.OpenAiApiKey);
//NB: Azure OpenAI is NOT SUPPORTED
#pragma warning disable OPENAI001
AIAgent agent = client
    //.GetChatClient("gpt-4.1")
    .GetOpenAIResponseClient("gpt-4.1")
#pragma warning restore OPENAI001
    .CreateAIAgent(
        instructions: "You are a Space News AI Reporter",
        tools: [new HostedWebSearchTool()]
    );

List<AgentRunResponseUpdate> updates = [];
string question = "What is today's news in Space Exploration (List today's date at the top)";
await foreach (AgentRunResponseUpdate update in agent.RunStreamingAsync(question))
{
    updates.Add(update);
    Console.Write(update);
}

AgentRunResponse fullResponse = updates.ToAgentRunResponse();
fullResponse.Usage.OutputAsInformation();