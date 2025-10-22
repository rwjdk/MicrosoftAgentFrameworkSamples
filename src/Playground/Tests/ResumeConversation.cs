using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI;
using OpenAI.Responses;
using Shared;
using Shared.Extensions;
using System.ClientModel;

#pragma warning disable OPENAI001

namespace Playground.Tests;

public class ResumeConversation
{
    public static async Task Run(Configuration configuration)
    {
        //OpenAIClient client = new(configuration.OpenAiApiKey);
        AzureOpenAIClient client = new(new Uri(configuration.AzureOpenAiEndpoint), new ApiKeyCredential(configuration.AzureOpenAiKey));
        OpenAIResponseClient responseClient = client.GetOpenAIResponseClient("gpt-4.1");
        AIAgent agent = responseClient
            .CreateAIAgent(
                instructions: "You are a Nice AI"
            );

        AgentThread thread = agent.GetNewThread();

        AgentRunResponse response1 = await agent.RunAsync("Who is Barak Obama? (Max 5 words)", thread);
        Console.WriteLine(response1);

        AgentRunResponse response2 = await agent.RunAsync("How Tall is he?", thread);
        Console.WriteLine(response2);

        //Imagine some time go by and user come back and the in-process thread is gone and not stored... Only the conversation ID
        string? responseId = response2.ResponseId;

        //Get previous text calling this multiple times
        //ClientResult<OpenAIResponse> result = await responseClient.GetResponseAsync(responseId);

        AgentRunResponse response3 = await agent.RunAsync("What city is he from", options: new ChatClientAgentRunOptions
        {
            ChatOptions = new ChatOptions
            {
                ConversationId = responseId
            }
        });
        Console.WriteLine(response3);
    }
}