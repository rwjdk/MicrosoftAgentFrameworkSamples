using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Shared;
using System.ClientModel;
using Microsoft.Extensions.AI;
using OpenAI;
using OpenAI.Chat;
using Shared.Extensions;
using ChatMessage = Microsoft.Extensions.AI.ChatMessage;

#pragma warning disable MEAI001

Console.Clear();
Secrets secrets = SecretManager.GetSecrets();

AzureOpenAIClient client = new(new Uri(secrets.AzureOpenAiEndpoint), new ApiKeyCredential(secrets.AzureOpenAiKey));
ChatClient chatClient = client.GetChatClient(secrets.ChatDeploymentName);

IChatReducer chatReducer = new MessageCountingChatReducer(targetCount: 4);
IChatReducer chatReducer2 = new SummarizingChatReducer(chatClient.AsIChatClient(), targetCount: 1, threshold: 4);

ChatClientAgent agent = client
    .GetChatClient(secrets.ChatDeploymentName)
    .CreateAIAgent(new ChatClientAgentOptions
    {
        ChatOptions = new()
        {
            Instructions = "You are a Friendly AI Bot, answering questions",
        },
        ChatMessageStoreFactory = context => new InMemoryChatMessageStore(chatReducer2, context.SerializedState, context.JsonSerializerOptions)
    });

AgentThread thread = agent.GetNewThread();

while (true)
{
    Console.Write("> ");
    string input = Console.ReadLine() ?? string.Empty;
    AgentRunResponse response = await agent.RunAsync(input, thread);
    Console.WriteLine(response);
    response.Usage.OutputAsInformation();

    IList<ChatMessage> messagesInThread = thread.GetService<IList<ChatMessage>>()!;
    Utils.WriteLineDarkGray("- Number of messages in thread: " + messagesInThread.Count());
    foreach (ChatMessage message in messagesInThread)
    {
        Utils.WriteLineDarkGray($"-- {message.Role}: {message.Text}");
    }

    Utils.Separator();
}