//YouTube video that cover this sample: https://youtu.be/arfBt6-wbnw

using Azure;
using Azure.AI.Agents.Persistent;
using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using OpenAI;
using Shared;
using System.ClientModel;

Configuration configuration = ConfigurationManager.GetConfiguration();

string questionToAsk = "What is the capital of the USA?";

await CreateAndCallNormalClientAgent("gpt-4.1-mini", questionToAsk); //Works
await CreateAndCallNormalClientAgent("DeepSeek-R1-0528", questionToAsk); //Works

await CreateAndCallFoundryAgent("gpt-4.1-mini", questionToAsk); //Works
await CreateAndCallFoundryAgent("DeepSeek-R1-0528", questionToAsk); //This does NOT work with non-OpenAI Models (but do also not throw an exception ?!?)

async Task CreateAndCallNormalClientAgent(string model, string question)
{
    AzureOpenAIClient client = new(new Uri(configuration.AzureOpenAiEndpoint), new ApiKeyCredential(configuration.AzureOpenAiKey));

    ChatClientAgent agent = client
        .GetChatClient(model)
        .CreateAIAgent(instructions: "You are a Friendly AI Bot, answering questions");

    AgentRunResponse response = await agent.RunAsync(question);
    Utils.WriteLineYellow($"Answer from ChatClient using Model: '{model}'");
    Console.WriteLine($"Answer ='{response}'");
    Utils.Separator();
}

async Task CreateAndCallFoundryAgent(string model, string question)
{
    PersistentAgentsClient client = new(configuration.AzureAiFoundryAgentEndpoint, new AzureCliCredential());
    string? agentIdToDelete = null;
    try
    {
        Response<PersistentAgent> foundryAgent = await client.Administration
            .CreateAgentAsync(model, $"MyFirstAgent for {model}", "Some description", "You are a nice AI");

        agentIdToDelete = foundryAgent.Value.Id;

        ChatClientAgent agent = await client.GetAIAgentAsync(foundryAgent.Value.Id);
        AgentRunResponse response = await agent.RunAsync(question);
        Utils.WriteLineYellow($"Answer from FoundryAgent using Model: '{model}'");
        Console.WriteLine($"Answer ='{response}'");
        Utils.Separator();
    }
    catch (Exception ex)
    {
        Utils.WriteLineRed("Error: " + ex);
    }
    finally
    {
        if (agentIdToDelete != null)
        {
            await client.Administration.DeleteAgentAsync(agentIdToDelete);
        }
    }
}