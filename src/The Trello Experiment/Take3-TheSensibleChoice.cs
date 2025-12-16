using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI;
using Shared;
using System.Text;
using The_Trello_Experiment.Tools;
using TrelloDotNet;

#pragma warning disable OPENAI001

namespace The_Trello_Experiment;

public static class Take3TheSensibleChoice
{
    public static async Task Run(AzureOpenAIClient azureOpenAIClient, string trelloApiKey, string trelloToken)
    {
        TrelloTools trelloTools = new(new TrelloClient(trelloApiKey, trelloToken));

        AIAgent agent = azureOpenAIClient
            .GetOpenAIResponseClient("gpt-4.1-mini")
            .CreateAIAgent(
                instructions: """
                              You are a Trello Expert 

                              Use the various tools to get Trello Information and do Actions
                              """,
                tools:
                [
                    AIFunctionFactory.Create(trelloTools.AddNewCard),
                    AIFunctionFactory.Create(trelloTools.GetBoards),
                    AIFunctionFactory.Create(trelloTools.GetCardsOnBoard),
                    AIFunctionFactory.Create(trelloTools.GetLabelsOnBoard),
                    AIFunctionFactory.Create(trelloTools.GetListsOnBoard),
                    AIFunctionFactory.Create(trelloTools.MoveCard),
                ]
            ).AsBuilder().Use(FunctionCallMiddleware).Build();

        AgentThread thread = agent.GetNewThread();

        while (true)
        {
            Console.Write("> ");
            string? input = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(input))
            {
                ChatMessage message = new(ChatRole.User, input);
                await foreach (AgentRunResponseUpdate update in agent.RunStreamingAsync(message, thread))
                {
                    Console.Write(update);
                }
            }

            Utils.Separator();
        }
    }

    static async ValueTask<object?> FunctionCallMiddleware(AIAgent callingAgent, FunctionInvocationContext context, Func<FunctionInvocationContext, CancellationToken, ValueTask<object?>> next, CancellationToken cancellationToken)
    {
        StringBuilder functionCallDetails = new();
        functionCallDetails.Append($"- Tool Call: '{context.Function.Name}'");
        if (context.Arguments.Count > 0)
        {
            functionCallDetails.Append($" (Args: {string.Join(",", context.Arguments.Select(x => $"[{x.Key} = {x.Value}]"))}");
        }

        Utils.WriteLineDarkGray(functionCallDetails.ToString());

        return await next(context, cancellationToken);
    }
}