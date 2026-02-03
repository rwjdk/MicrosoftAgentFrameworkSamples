using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using OpenAI.Chat;
using Shared;
using System.ClientModel;

namespace WhyIDontUseWorkflows.WorkflowTypes;

public static class HandoffWithoutWorkflowStructuredOutputEdition
{
    public static async Task Run()
    {
        Secrets secrets = SecretManager.GetSecrets();

        AzureOpenAIClient client = new(new Uri(secrets.AzureOpenAiEndpoint), new ApiKeyCredential(secrets.AzureOpenAiKey));

        ChatClientAgent intentAgent = client.GetChatClient("gpt-4.1-mini").AsAIAgent(name: "IntentAgent", instructions: "Determine what type of question was asked. Never answer yourself");

        ChatClientAgent movieNerd = client.GetChatClient("gpt-4.1").AsAIAgent(name: "MovieNerd", instructions: "You are a Movie Nerd");
        ChatClientAgent musicNerd = client.GetChatClient("gpt-4.1").AsAIAgent(name: "MusicNerd", instructions: "You are a Music Nerd");

        Console.Write("> ");
        var input = Console.ReadLine()!;

        var intentResponse = await intentAgent.RunAsync<TypeOfQuestion>(input);
        switch (intentResponse.Result)
        {
            case TypeOfQuestion.Movie:
                {
                    Utils.WriteLineGreen(movieNerd.Name!);
                    var response = await movieNerd.RunAsync(input);
                    Console.WriteLine(response);
                    break;
                }
            case TypeOfQuestion.Music:
                {
                    Utils.WriteLineGreen(musicNerd.Name!);
                    var response = await musicNerd.RunAsync(input);
                    Console.WriteLine(response);
                    break;
                }
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}

enum TypeOfQuestion
{
    Movie,
    Music
}