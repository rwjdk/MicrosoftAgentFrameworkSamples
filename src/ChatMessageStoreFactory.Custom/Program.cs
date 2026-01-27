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
            ChatHistoryProviderFactory = (context, token) => ValueTask.FromResult<ChatHistoryProvider>(new MyMessageStore(context))
        }
    );

AgentSession session = await agent.GetNewSessionAsync();

AgentResponse response = await agent.RunAsync("Who is Barack Obama", session);
Console.WriteLine(response);

JsonElement sessionElement = session.Serialize();
string toStoreForTheUser = JsonSerializer.Serialize(sessionElement);

Utils.Separator();

//Some time passes.... 
Utils.WriteLineGreen("Some time passes, and we restore the session...");


JsonElement restoredSessionElement = JsonSerializer.Deserialize<JsonElement>(toStoreForTheUser);

AgentSession restoredThread = await agent.DeserializeSessionAsync(restoredSessionElement);

AgentResponse someTimeLaterResponse = await agent.RunAsync("How Tall is he?", restoredThread);
Console.WriteLine(someTimeLaterResponse);

class MyMessageStore(ChatClientAgentOptions.ChatHistoryProviderFactoryContext factoryContext) : ChatHistoryProvider
{
    public string SessionId { get; set; } = factoryContext.SerializedState.ValueKind is JsonValueKind.String ? factoryContext.SerializedState.Deserialize<string>()! : Guid.NewGuid().ToString();

    public string SessionPath => Path.Combine(Path.GetTempPath(), $"{SessionId}.json");

    private readonly List<ChatMessage> _messages = [];

    public override async ValueTask<IEnumerable<ChatMessage>> InvokingAsync(InvokingContext context, CancellationToken cancellationToken = new CancellationToken())
    {
        if (!File.Exists(SessionPath))
        {
            return [];
        }

        string json = await File.ReadAllTextAsync(SessionPath, cancellationToken);
        return JsonSerializer.Deserialize<List<ChatMessage>>(json)!;
    }

    public override async ValueTask InvokedAsync(InvokedContext context, CancellationToken cancellationToken = new CancellationToken())
    {
        // Add both request and response messages to the store
        // Optionally messages produced by the AIContextProvider can also be persisted (not shown).
        _messages.AddRange(context.RequestMessages.Concat(context.AIContextProviderMessages ?? []).Concat(context.ResponseMessages ?? []));

        await File.WriteAllTextAsync(SessionPath, JsonSerializer.Serialize(_messages, factoryContext.JsonSerializerOptions), cancellationToken);
    }

    public override JsonElement Serialize(JsonSerializerOptions? jsonSerializerOptions = null)
    {
        return JsonSerializer.SerializeToElement(SessionId, factoryContext.JsonSerializerOptions);
    }
}