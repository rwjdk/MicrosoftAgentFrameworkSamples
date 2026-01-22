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
    public static async Task Run(Secrets secrets)
    {
        //OpenAIClient client = new(secrets.OpenAiApiKey);
        AzureOpenAIClient client = new(new Uri(secrets.AzureOpenAiEndpoint), new ApiKeyCredential(secrets.AzureOpenAiKey));
        ResponsesClient responseClient = client.GetResponsesClient("gpt-4.1");
        AIAgent agent = responseClient
            .AsAIAgent(
                instructions: "You are a Nice AI"
            );

        AgentThread thread = await agent.GetNewThreadAsync();

        AgentResponse response1 = await agent.RunAsync("Who is Barak Obama? (Max 5 words)", thread);
        Console.WriteLine(response1);

        AgentResponse response2 = await agent.RunAsync("How Tall is he?", thread);
        Console.WriteLine(response2);

        //Imagine some time go by and user come back and the in-process thread is gone and not stored... Only the conversation ID
        string? responseId = response2.ResponseId;

        //Get previous text calling this multiple times
        //ClientResult<OpenAIResponse> result = await responseClient.GetResponseAsync(responseId);

        AgentResponse response3 = await agent.RunAsync("What city is he from", options: new ChatClientAgentRunOptions
        {
            ChatOptions = new ChatOptions
            {
                ConversationId = responseId
            }
        });
        Console.WriteLine(response3);
    }
}