using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Shared;
using System.ClientModel;
using System.Text.Json;
using Microsoft.Extensions.AI;
using OpenAI.Chat;
using ChatMessage = Microsoft.Extensions.AI.ChatMessage;

Console.Clear();

Secrets secrets = SecretManager.GetSecrets();

AzureOpenAIClient azureOpenAIClient = new(new Uri(secrets.AzureOpenAiEndpoint), new ApiKeyCredential(secrets.AzureOpenAiKey));

ChatClientAgent agent = azureOpenAIClient
    .GetChatClient("gpt-4.1-mini")
    .CreateAIAgent(
        new ChatClientAgentOptions
        {
            ChatMessageStoreFactory = context => new MyMessageStore(context)
        }
    );

AgentThread thread = agent.GetNewThread();

AgentRunResponse response = await agent.RunAsync("Who is Barack Obama", thread);
Console.WriteLine(response);

JsonElement threadElement = thread.Serialize();
string toStoreForTheUser = JsonSerializer.Serialize(threadElement);

//Some time passes.... 

JsonElement restoredThreadElement = JsonSerializer.Deserialize<JsonElement>(toStoreForTheUser);

AgentThread restoredThread = agent.DeserializeThread(restoredThreadElement);

AgentRunResponse someTimeLaterResponse = await agent.RunAsync("How Tall is he?", restoredThread);
Console.WriteLine(someTimeLaterResponse);

class MyMessageStore(ChatClientAgentOptions.ChatMessageStoreFactoryContext context) : ChatMessageStore
{
    public string ThreadId { get; set; } = context.SerializedState.ValueKind is JsonValueKind.String ? context.SerializedState.Deserialize<string>()! : Guid.NewGuid().ToString();

    public string ThreadPath => Path.Combine(Path.GetTempPath(), $"{ThreadId}.json");

    private readonly List<ChatMessage> _messages = [];

    public override async Task<IEnumerable<ChatMessage>> GetMessagesAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(ThreadPath))
        {
            return [];
        }

        string json = await File.ReadAllTextAsync(ThreadPath, cancellationToken);
        return JsonSerializer.Deserialize<List<ChatMessage>>(json)!;
    }

    public override async Task AddMessagesAsync(IEnumerable<ChatMessage> messages, CancellationToken cancellationToken = new CancellationToken())
    {
        _messages.AddRange(messages);
        await File.WriteAllTextAsync(ThreadPath, JsonSerializer.Serialize(_messages, context.JsonSerializerOptions), cancellationToken);
    }

    public override JsonElement Serialize(JsonSerializerOptions? jsonSerializerOptions = null)
    {
        return JsonSerializer.SerializeToElement(ThreadId, context.JsonSerializerOptions);
    }
}