using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI.Chat;
using Shared;
using System.ClientModel;
using System.Text;

namespace WhyIDontUseWorkflows.WorkflowTypes;

public static class HandoffWithoutWorkflowToolEdition
{
    public static async Task Run()
    {
        Secrets secrets = SecretManager.GetSecrets();

        AzureOpenAIClient client = new(new Uri(secrets.AzureOpenAiEndpoint), new ApiKeyCredential(secrets.AzureOpenAiKey));

        ChatClientAgent movieNerd = client.GetChatClient("gpt-4.1").AsAIAgent(name: "MovieNerd", instructions: "You are a Movie Nerd");
        ChatClientAgent musicNerd = client.GetChatClient("gpt-4.1").AsAIAgent(name: "MusicNerd", instructions: "You are a Music Nerd");

        AIAgent intentAgent = client.GetChatClient("gpt-4.1-mini")
            .AsAIAgent(name: "IntentAgent", 
                instructions: "Determine what type of question was asked and use tool. Never answer yourself",
                tools: [movieNerd.AsAIFunction(), musicNerd.AsAIFunction()]).AsBuilder().Use(FunctionCallMiddleware).Build();
        
        Console.Write("> ");
        var input = Console.ReadLine()!;

        var response = await intentAgent.RunAsync(input);
        Console.WriteLine(response);
    }

    static async ValueTask<object?> FunctionCallMiddleware(AIAgent callingAgent, FunctionInvocationContext context, Func<FunctionInvocationContext, CancellationToken, ValueTask<object?>> next, CancellationToken cancellationToken)
    {
        StringBuilder functionCallDetails = new();
        functionCallDetails.Append($"- Tool Call: '{context.Function.Name}'");
        if (context.Arguments.Count > 0)
        {
            functionCallDetails.Append($" (Args: {string.Join(",", context.Arguments.Select(x => $"[{x.Key} = {x.Value}]"))}");
        }

        Utils.Gray(functionCallDetails.ToString());

        return await next(context, cancellationToken);
    }
}