//YouTube video that cover this sample: https://youtu.be/2YzjRZTZxUo

using System.ClientModel;
using System.ComponentModel;
using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using OpenAI;
using OpenAI.Chat;
using Shared;

Configuration configuration = ConfigurationManager.GetConfiguration();

AzureOpenAIClient client = new(new Uri(configuration.AzureOpenAiEndpoint), new ApiKeyCredential(configuration.AzureOpenAiKey));

ChatClient chatClientMini = client.GetChatClient("gpt-4.1-mini");
ChatClient chatClient = client.GetChatClient("gpt-4.1");

Console.Write("> ");
string question = Console.ReadLine()!;

//Determine initial intent
ChatClientAgent intentAgent = chatClientMini.CreateAIAgent(name: "IntentAgent", instructions: "Determine what type of question was asked. Never answer yourself");

AgentRunResponse<IntentResult> initialResponse = await intentAgent.RunAsync<IntentResult>(question);
IntentResult intentResult = initialResponse.Result;

//Branch out based on Intent
switch (intentResult.Intent)
{
    case Intent.MusicQuestion:
        Utils.WriteLineGreen("Music Question");
        ChatClientAgent musicNerdAgent = chatClient.CreateAIAgent(name: "MusicNerd", instructions: "You are a Music Nerd answering questions (Give a question on max 200 chars)");
        AgentRunResponse responseFromMusicNerd = await musicNerdAgent.RunAsync(question);
        Console.WriteLine(responseFromMusicNerd);
        break;
    case Intent.MovieQuestion:
        Utils.WriteLineGreen("Movie Question");
        ChatClientAgent movieNerdAgent = chatClient.CreateAIAgent(name: "MovieNerd", instructions: "You are a Movie Nerd answering questions (Give a question on max 200 chars)");
        AgentRunResponse responseFromMovieNerd = await movieNerdAgent.RunAsync(question);
        Console.WriteLine(responseFromMovieNerd);
        break;
    case Intent.Other:
        Utils.WriteLineGreen("Other Question");
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