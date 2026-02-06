//YouTube video that cover this sample: https://youtu.be/p5AvoMbgPtI
// ReSharper disable HeuristicUnreachableCode

#pragma warning disable CS0162 // Unreachable code detected
using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using OpenAI;
using Shared;
using System.ClientModel;
using ConversationThreads;
using Microsoft.Extensions.AI;
using OpenAI.Chat;
using ChatMessage = Microsoft.Extensions.AI.ChatMessage;

Secrets secrets = SecretManager.GetSecrets();

AzureOpenAIClient client = new(new Uri(secrets.AzureOpenAiEndpoint), new ApiKeyCredential(secrets.AzureOpenAiKey));

var agent = client
    .GetChatClient(secrets.ChatDeploymentName)
    .AsAIAgent(instructions: "You are a Friendly AI Bot, answering questions");

AgentSession session;

const bool optionToResume = true; //Set this to true to test resume of previous conversations

if (optionToResume)
{
    session = await AgentThreadPersistence.ResumeChatIfRequestedAsync(agent);
}
else
{
    session = await agent.CreateSessionAsync();
}

while (true)
{
    Console.Write("> ");
    string? input = Console.ReadLine();
    if (!string.IsNullOrWhiteSpace(input))
    {
        ChatMessage message = new(ChatRole.User, input);
        await foreach (AgentResponseUpdate update in agent.RunStreamingAsync(message, session))
        {
            Console.Write(update);
        }
    }

    Utils.Separator();

    if (optionToResume)
    {
        await AgentThreadPersistence.StoreThreadAsync(agent, session);
    }
}