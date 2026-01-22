using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using OpenAI.Chat;
using Shared;
using System.ClientModel;
using System.Text.Json;
using ChatMessage = Microsoft.Extensions.AI.ChatMessage;

Console.Clear();

Secrets secrets = SecretManager.GetSecrets();

AzureOpenAIClient azureOpenAIClient = new(new Uri(secrets.AzureOpenAiEndpoint), new ApiKeyCredential(secrets.AzureOpenAiKey));

ChatClientAgent agent = azureOpenAIClient
    .GetChatClient("gpt-4.1-mini")
    .AsAIAgent(
        new ChatClientAgentOptions
        {
            ChatMessageStoreFactory = (context, token) => ValueTask.FromResult<ChatMessageStore>(new MyMessageStore(context))
        }
    );

AgentThread thread = await agent.GetNewThreadAsync();

AgentResponse response = await agent.RunAsync("Who is Barack Obama", thread);
Console.WriteLine(response);

JsonElement threadElement = thread.Serialize();
string toStoreForTheUser = JsonSerializer.Serialize(threadElement);

Utils.Separator();

//Some time passes.... 
Utils.WriteLineGreen("Some time passes, and we restore the thread...");


JsonElement restoredThreadElement = JsonSerializer.Deserialize<JsonElement>(toStoreForTheUser);

AgentThread restoredThread = await agent.DeserializeThreadAsync(restoredThreadElement);

AgentResponse someTimeLaterResponse = await agent.RunAsync("How Tall is he?", restoredThread);
Console.WriteLine(someTimeLaterResponse);

class MyMessageStore(ChatClientAgentOptions.ChatMessageStoreFactoryContext factoryContext) : ChatMessageStore
{
    public string ThreadId { get; set; } = factoryContext.SerializedState.ValueKind is JsonValueKind.String ? factoryContext.SerializedState.Deserialize<string>()! : Guid.NewGuid().ToString();

    public string ThreadPath => Path.Combine(Path.GetTempPath(), $"{ThreadId}.json");

    private readonly List<ChatMessage> _messages = [];

    public override async ValueTask<IEnumerable<ChatMessage>> InvokingAsync(InvokingContext context, CancellationToken cancellationToken = new CancellationToken())
    {
        if (!File.Exists(ThreadPath))
        {
            return [];
        }

        string json = await File.ReadAllTextAsync(ThreadPath, cancellationToken);
        return JsonSerializer.Deserialize<List<ChatMessage>>(json)!;
    }

    public override async ValueTask InvokedAsync(InvokedContext context, CancellationToken cancellationToken = new CancellationToken())
    {
        // Add both request and response messages to the store
        // Optionally messages produced by the AIContextProvider can also be persisted (not shown).
        _messages.AddRange(context.RequestMessages.Concat(context.AIContextProviderMessages ?? []).Concat(context.ResponseMessages ?? []));

        await File.WriteAllTextAsync(ThreadPath, JsonSerializer.Serialize(_messages, factoryContext.JsonSerializerOptions), cancellationToken);
    }

    public override JsonElement Serialize(JsonSerializerOptions? jsonSerializerOptions = null)
    {
        return JsonSerializer.SerializeToElement(ThreadId, factoryContext.JsonSerializerOptions);
    }
}