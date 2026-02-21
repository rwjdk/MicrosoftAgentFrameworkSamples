using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Shared;
using System.ClientModel;
using System.Text.Json;
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
    .AsAIAgent(new ChatClientAgentOptions
    {
        ChatOptions = new()
        {
            Instructions = "You are a Friendly AI Bot, answering questions",
        },
        ChatHistoryProvider = new InMemoryChatHistoryProvider(new InMemoryChatHistoryProviderOptions
        {
            ChatReducer = chatReducer2,
        })
    });

AgentSession session = await agent.CreateSessionAsync();

while (true)
{
    Console.Write("> ");
    string input = Console.ReadLine() ?? string.Empty;
    AgentResponse response = await agent.RunAsync(input, session);
    Console.WriteLine(response);
    response.Usage.OutputAsInformation();

    Utils.WriteLineDarkGray((await agent.SerializeSessionAsync(session)).GetRawText()); //todo - temp workaround
    /* this does not work in RC1 - as Team for reason here: https://github.com/microsoft/agent-framework/issues/4140
    IList<ChatMessage> messagesInSession = session.GetService<IList<ChatMessage>>()!;
    Utils.WriteLineDarkGray("- Number of messages in session: " + messagesInSession.Count());
    foreach (ChatMessage message in messagesInSession)
    {
        Utils.WriteLineDarkGray($"-- {message.Role}: {message.Text}");
    }
    */
    Utils.Separator();
}