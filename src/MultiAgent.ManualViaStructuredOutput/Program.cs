using System.ClientModel;
using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI;
using OpenAI.Chat;
using Shared;
using ChatResponseFormat = Microsoft.Extensions.AI.ChatResponseFormat;

Configuration configuration = ConfigurationManager.GetConfiguration();

AzureOpenAIClient client = new(new Uri(configuration.AzureOpenAiEndpoint), new ApiKeyCredential(configuration.AzureOpenAiKey));

ChatClient chatClientMini = client.GetChatClient("gpt-4.1-mini");
ChatClient chatClient = client.GetChatClient("gpt-4.1");

Console.Write("> ");
string question = Console.ReadLine()!;

//Determine initial intent
AIAgent intentAgent = chatClientMini.CreateAIAgent(name: "IntentAgent", instructions: "Determine what type of question was asked. Never answer yourself");

JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions
{
    PropertyNameCaseInsensitive = true,
    TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
    Converters = { new JsonStringEnumConverter() }
};

AgentRunResponse initialResponse = await intentAgent.RunAsync(question, options: new ChatClientAgentRunOptions()
{
    ChatOptions = new ChatOptions
    {
        ResponseFormat = ChatResponseFormat.ForJsonSchema<IntentResult>(jsonSerializerOptions)
    }
});
IntentResult intentResult = initialResponse.Deserialize<IntentResult>(jsonSerializerOptions);

//Branch out based on Intent
switch (intentResult.Intent)
{
    case Intent.MusicQuestion:
        Utils.WriteLineSuccess("Music Question");
        AIAgent musicNerdAgent = chatClient.CreateAIAgent(name: "MusicNerd", instructions: "You are a Music Nerd (Give a question on max 200 chars)");
        AgentRunResponse responseFromMusicNerd = await musicNerdAgent.RunAsync(question);
        Console.WriteLine(responseFromMusicNerd);
        break;
    case Intent.MovieQuestion:
        Utils.WriteLineSuccess("Movie Question");
        AIAgent movieNerdAgent = chatClient.CreateAIAgent(name: "MovieNerd", instructions: "You are a Movie Nerd (Give a question on max 200 chars)");
        AgentRunResponse responseFromMovieNerd = await movieNerdAgent.RunAsync(question);
        Console.WriteLine(responseFromMovieNerd);
        break;
    case Intent.Other:
        Utils.WriteLineSuccess("Other Question");
        //Let Intent agent answer itself
        AgentRunResponse otherResponse = await intentAgent.RunAsync(question);
        Console.WriteLine(otherResponse);
        break;
    default:
        throw new ArgumentOutOfRangeException();
}

Console.ReadKey();

public class IntentResult
{
    [Description("What type of question is this?")]
    public required Intent Intent { get; set; }
}

public enum Intent
{
    MusicQuestion,
    MovieQuestion,
    Other
}