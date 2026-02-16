using AgentFrameworkToolkit.Tools;
using AgentFrameworkToolkit.Tools.ModelContextProtocol;
using Google.GenAI;
using Google.GenAI.Types;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared;

Secrets secrets = SecretManager.GetSecrets();
Client client = new Google.GenAI.Client(apiKey: secrets.GoogleGeminiApiKey, httpOptions: new HttpOptions
{
    Timeout = Convert.ToInt32(TimeSpan.FromMinutes(5).TotalMilliseconds) // 👍👍👍 was not possible in the unofficial NuGet
    // 👎👎👎: We still can't access raw HTTP to intercept RAW Calls
});

IChatClient iChatClient = client
    .AsIChatClient("gemini-3-pro-preview");

//----------------------------------------------------------------------------------------------------------------

//Normal Agent
ChatClientAgent chatAgent = new ChatClientAgent(iChatClient);
AgentResponse chatResponse = await chatAgent.RunAsync("What is the capital of France?");
Console.WriteLine(chatResponse);

//----------------------------------------------------------------------------------------------------------------

ServiceCollection services = new ServiceCollection();
services.AddSingleton<HttpClient>(new HttpClient());
IServiceProvider serviceProvider = services.BuildServiceProvider();

//Streaming + Tools (Was an issue in the unofficial nuget)
ChatClientAgent toolAgent = new ChatClientAgent(iChatClient,
    tools: [AIFunctionFactory.Create(GetWeather)],
    services: serviceProvider // 👍👍👍 was not possible in the unofficial NuGet
);

List<AgentResponseUpdate> updates = [];
await foreach (AgentResponseUpdate update in toolAgent.RunStreamingAsync("What is the weather like in paris?"))
{
    updates.Add(update);
    Console.Write(update); // 👍👍👍 was not possible in the unofficial NuGet
}

AgentResponse response = updates.ToAgentResponse();
foreach (ChatMessage message in response.Messages)
{
    Console.WriteLine("- Role: " + message.Role); // 👍👍👍 was not possible in the unofficial NuGet
}

//----------------------------------------------------------------------------------------------------------------

//MCP Tools
AIToolsFactory toolsFactory = new AIToolsFactory();
await using McpClientTools mcpClientTools = await toolsFactory.GetToolsFromRemoteMcpAsync("https://trellodotnetassistantbackend.azurewebsites.net/runtime/webhooks/mcp?code=Tools");
ChatClientAgent mcpToolAgent = new ChatClientAgent(iChatClient, tools: mcpClientTools.Tools);

AgentResponse mcpToolResponse = await mcpToolAgent.RunAsync("Call the 'getting_started' tool to find what URL the nuget is on");
Console.WriteLine(mcpToolResponse); // 👍👍👍 was not possible in the unofficial NuGet

//----------------------------------------------------------------------------------------------------------------

//Structured Output
ChatClientAgent structuredOutputAgent = new ChatClientAgent(iChatClient); //This did not work with the unofficial NuGet
ChatClientAgentResponse<MovieResult> structuredOutputResponse = await structuredOutputAgent.RunAsync<MovieResult>("List top 3 movies according to IMDB");
Console.WriteLine(structuredOutputResponse);

//----------------------------------------------------------------------------------------------------------------

//Thinking Settings Agent
ChatClientAgent agent = new ChatClientAgent(iChatClient, new ChatClientAgentOptions
{
    ChatOptions = new ChatOptions
    {
        RawRepresentationFactory = chatClient => new GenerateContentConfig
        {
            ThinkingConfig = new ThinkingConfig
            {
                /*Thinking Budget
                - is only recommended for Models older than Gemini 3
                -1 = Auto, 0 = Disable (Gemini 3 can't turn off), Other number (Max tokens)
                */
                //ThinkingBudget = 100

                /* Thinking Level
                 * - Gemini 3 and higher use this instead of
                 *   - Pro support HIGH and LOW
                 *   - Flash support HIGH, MEDIUM, LOW and MINIMAL
                 */
                ThinkingLevel = ThinkingLevel.High,

                //Including the reasoning
                IncludeThoughts = true // 👍👍👍 was not possible in the unofficial NuGet
            }
        }
    }
});

AgentResponse thinkingResponse = await agent.RunAsync("Why is the Sky Blue");
Console.WriteLine(thinkingResponse);

Console.WriteLine("- Reasoning Token: " + thinkingResponse.Usage!.ReasoningTokenCount); // 👍👍👍 was not possible in the unofficial NuGet

foreach (ChatMessage message in thinkingResponse.Messages)
{
    foreach (AIContent content in message.Contents)
    {
        if (content is TextReasoningContent textReasoningContent)
        {
            Console.WriteLine("- Reasoning Text: " + textReasoningContent.Text);
        }
    }
}

//--------------------------------------------------------------------------------------------------


IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator = client.Models.AsIEmbeddingGenerator("gemini-embedding-001");
Embedding<float> generateAsync = await embeddingGenerator.GenerateAsync("Hello World"); // 👍👍👍 was not possible in the unofficial NuGet

//--------------------------------------------------------------------------------------------------

static string GetWeather(IServiceProvider serviceProvider, string city)
{
    HttpClient requiredService = serviceProvider.GetRequiredService<HttpClient>();
    return "{ \"condition\": \"sunny\", \"degrees\":19 }";
}

public class MovieResult
{
    public required List<Movie> Movies { get; set; }
}

public class Movie
{
    public required string Title { get; set; }
    public required string Director { get; set; }
    public required int YearOfRelease { get; set; }
    public required decimal ImdbScore { get; set; }
    public required MovieGenre Genre { get; set; }
}

public enum MovieGenre
{
    Drama,
    SciFi,
    Adventure,
    Other
}