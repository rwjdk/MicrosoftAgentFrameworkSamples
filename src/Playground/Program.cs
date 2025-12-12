//WARNING: This is a playground area for the creator of the Repo to test and tinker. Nothing in this project is as such educational and might not even execute properly

//Notes
//- Microsoft.Agents.AI.Hosting.AgentCatalog TODO: Guess this is something to be used in AI Foundry

#pragma warning disable OPENAI001
using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI;
using Shared;
using System.ClientModel;
using System.Text.Json;

Console.WriteLine("");
Console.Clear();

Secrets secrets = SecretManager.GetConfiguration();

AzureOpenAIClient azureOpenAIClient = new AzureOpenAIClient(new Uri(secrets.AzureOpenAiEndpoint), new ApiKeyCredential(secrets.AzureOpenAiKey));

ChatClientAgent a = azureOpenAIClient
    .GetChatClient("gpt-4.1")
    .CreateAIAgent(
        new ChatClientAgentOptions
        {
            ChatMessageStoreFactory = context => new MyMessageStore(context)
        }
    );


AgentThread thread = a.GetNewThread();

AgentRunResponse response = await a.RunAsync("Who is Barack Obama", thread);
Console.WriteLine(response);

JsonElement threadElement = thread.Serialize();
string toStoreForTheUser = JsonSerializer.Serialize(threadElement);

//Some time passes.... 

JsonElement restoredThreadElement = JsonSerializer.Deserialize<JsonElement>(toStoreForTheUser);

AgentThread restoredThread = a.DeserializeThread(restoredThreadElement);

AgentRunResponse someTimeLaterResponse = await a.RunAsync("How Tall is he?", restoredThread);
Console.WriteLine(someTimeLaterResponse);


//await AzureOpenAiFoundry.Run(configuration);
//await FileTool.Run(configuration);
//await CodeTool.Run(configuration);
//await ReasoningSummary.Run(configuration);
//await CodexSpecialModels.Run(configuration);
//await SpaceNewsWebSearch.Run(configuration);
//await ResumeConversation.Run(configuration);
//await AzureOpenAiCodex.Run(configuration);


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

    public override async Task AddMessagesAsync(IEnumerable<ChatMessage> messages, CancellationToken cancellationToken = default)
    {
        _messages.AddRange(messages);
        await File.WriteAllTextAsync(ThreadPath, JsonSerializer.Serialize(_messages, context.JsonSerializerOptions), cancellationToken);
    }

    public override JsonElement Serialize(JsonSerializerOptions? jsonSerializerOptions = null)
    {
        return JsonSerializer.SerializeToElement(ThreadId, context.JsonSerializerOptions);
    }
}