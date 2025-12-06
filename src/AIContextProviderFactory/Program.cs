using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Shared;
using System.ClientModel;
using JetBrains.Annotations;
using OpenAI;

Console.Clear();
Configuration configuration = ConfigurationManager.GetConfiguration();

string userId = "rwj1234";

AzureOpenAIClient client = new(new Uri(configuration.AzureOpenAiEndpoint), new ApiKeyCredential(configuration.AzureOpenAiKey));

ChatClientAgent memoryExtractorAgent = client
    .GetChatClient("gpt-4.1-nano")
    .CreateAIAgent(
        instructions: "Look at the user's message and extract any memory that we do not already know (or non if there aren't any memories to store)"
    );

ChatClientAgent agentWithCustomMemory = client.GetChatClient(configuration.ChatDeploymentName).AsIChatClient()
    .CreateAIAgent(new ChatClientAgentOptions
    {
        ChatOptions = new()
        {
            Instructions = "You are a nice AI"
        },
        AIContextProviderFactory = _ => new CustomContextProvider(memoryExtractorAgent, userId)
    });

AIAgent agentToUse = agentWithCustomMemory;

AgentThread thread = agentToUse.GetNewThread();

while (true)
{
    Console.Write("> ");
    string? input = Console.ReadLine();
    if (!string.IsNullOrWhiteSpace(input))
    {
        ChatMessage message = new(ChatRole.User, input);
        AgentRunResponse response = await agentToUse.RunAsync(message, thread);
        {
            Console.WriteLine(response);
        }
    }

    Utils.Separator();
}

class CustomContextProvider : AIContextProvider
{
    private readonly ChatClientAgent _memoryExtractorAgent;
    private readonly List<string> _userFacts = [];
    private readonly string _userMemoryFilePath;

    public CustomContextProvider(ChatClientAgent memoryExtractorAgent, string userId)
    {
        _memoryExtractorAgent = memoryExtractorAgent;
        _userMemoryFilePath = Path.Combine(Path.GetTempPath(), $"{userId}.txt");
        if (File.Exists(_userMemoryFilePath))
        {
            _userFacts.AddRange(File.ReadAllLines(_userMemoryFilePath));
        }
    }

    public override ValueTask<AIContext> InvokingAsync(InvokingContext context, CancellationToken cancellationToken = default)
    {
        return new ValueTask<AIContext>(new AIContext
        {
            Instructions = string.Join(" | ", _userFacts)
        });
    }

    public override async ValueTask InvokedAsync(InvokedContext context, CancellationToken cancellationToken = new CancellationToken())
    {
        ChatMessage lastMessageFromUser = context.RequestMessages.Last();
        List<ChatMessage> inputToMemoryExtractor =
        [
            new(ChatRole.Assistant, $"We know the following about the user already and should not extract that again: {string.Join(" | ", _userFacts)}"),
            lastMessageFromUser
        ];

        ChatClientAgentRunResponse<MemoryUpdate> response = await _memoryExtractorAgent.RunAsync<MemoryUpdate>(inputToMemoryExtractor, cancellationToken: cancellationToken);
        foreach (string memoryToRemove in response.Result.MemoryToRemove)
        {
            _userFacts.Remove(memoryToRemove);
        }

        _userFacts.AddRange(response.Result.MemoryToAdd);
        await File.WriteAllLinesAsync(_userMemoryFilePath, _userFacts.Distinct(), cancellationToken);
    }

    [UsedImplicitly]
    private record MemoryUpdate(List<string> MemoryToAdd, List<string> MemoryToRemove);
}