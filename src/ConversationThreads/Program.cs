//YouTube video that cover this sample: https://youtu.be/p5AvoMbgPtI

using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using OpenAI;
using Shared;
using System.ClientModel;
using ConversationThreads;
using Microsoft.Extensions.AI;
using ChatMessage = Microsoft.Extensions.AI.ChatMessage;

Configuration configuration = ConfigurationManager.GetConfiguration();

AzureOpenAIClient client = new AzureOpenAIClient(new Uri(configuration.AzureOpenAiEndpoint), new ApiKeyCredential(configuration.AzureOpenAiKey));

AIAgent agent = client
    .GetChatClient(configuration.ChatDeploymentName)
    .CreateAIAgent(instructions: "You are a Friendly AI Bot, answering questions");

AgentThread thread = agent.GetNewThread();

const bool optionToResume = true; //Set this to true to test resume of previous conversations

if (optionToResume)
{
    thread = await AgentThreadPersistence.ResumeChatIfRequestedAsync(agent);
}

while (true)
{
    Console.Write("> ");
    string? input = Console.ReadLine();
    if (!string.IsNullOrWhiteSpace(input))
    {
        ChatMessage message = new ChatMessage(ChatRole.User, input);
        await foreach (AgentRunResponseUpdate update in agent.RunStreamingAsync(message, thread))
        {
            Console.Write(update);
        }
    }

    Utils.Separator();

    if (optionToResume)
    {
        await AgentThreadPersistence.StoreThreadAsync(thread);
    }
}