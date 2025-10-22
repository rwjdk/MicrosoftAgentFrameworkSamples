//YouTube video that cover this sample: https://youtu.be/wL4V78s_wI4

using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using MultiAgent.AgentAsTool;
using OpenAI;
using Shared;
using Shared.Extensions;
using System.ClientModel;
using System.Text;

Configuration configuration = ConfigurationManager.GetConfiguration();

AzureOpenAIClient client = new(new Uri(configuration.AzureOpenAiEndpoint), new ApiKeyCredential(configuration.AzureOpenAiKey));

AIAgent stringAgent = client
    .GetChatClient(configuration.ChatDeploymentName)
    .CreateAIAgent(
        name: "StringAgent",
        instructions: "You are string manipulator",
        tools:
        [
            AIFunctionFactory.Create(StringTools.Reverse),
            AIFunctionFactory.Create(StringTools.Uppercase),
            AIFunctionFactory.Create(StringTools.Lowercase)
        ])
    .AsBuilder()
    .Use(FunctionCallMiddleware)
    .Build();

AIAgent numberAgent = client
    .GetChatClient(configuration.ChatDeploymentName)
    .CreateAIAgent(
        name: "NumberAgent",
        instructions: "You are a number expert",
        tools:
        [
            AIFunctionFactory.Create(NumberTools.RandomNumber),
            AIFunctionFactory.Create(NumberTools.AnswerToEverythingNumber)
        ])
    .AsBuilder()
    .Use(FunctionCallMiddleware) //Middleware
    .Build();

Utils.WriteLineGreen("DELEGATE AGENT");

AIAgent delegationAgent = client
    .GetChatClient(configuration.ChatDeploymentName)
    .CreateAIAgent(
        name: "DelegateAgent",
        instructions: "Are a Delegator of String and Number Tasks. Never does such work yourself",
        tools:
        [
            stringAgent.AsAIFunction(new AIFunctionFactoryOptions
            {
                Name = "StringAgentAsTool"
            }),
            numberAgent.AsAIFunction(new AIFunctionFactoryOptions
            {
                Name = "NumberAgentAsTool"
            })
        ]
    )
    .AsBuilder()
    .Use(FunctionCallMiddleware)
    .Build();

AgentRunResponse responseFromDelegate = await delegationAgent.RunAsync("Uppercase 'Hello World'");
Console.WriteLine(responseFromDelegate);
responseFromDelegate.Usage.OutputAsInformation();

Utils.Separator();

Utils.WriteLineGreen("JACK OF ALL TRADE AGENT");

AIAgent jackOfAllTradesAgent = client
    .GetChatClient(configuration.ChatDeploymentName)
    .CreateAIAgent(
        name: "JackOfAllTradesAgent",
        instructions: "Are a Agent that can answer questions on strings and numbers",
        tools:
        [
            AIFunctionFactory.Create(StringTools.Reverse),
            AIFunctionFactory.Create(StringTools.Uppercase),
            AIFunctionFactory.Create(StringTools.Lowercase),
            AIFunctionFactory.Create(NumberTools.RandomNumber),
            AIFunctionFactory.Create(NumberTools.AnswerToEverythingNumber)
        ]
    )
    .AsBuilder()
    .Use(FunctionCallMiddleware)
    .Build();

AgentRunResponse responseFromJackOfAllTrade = await jackOfAllTradesAgent.RunAsync("Uppercase 'Hello World'");
Console.WriteLine(responseFromJackOfAllTrade);
responseFromJackOfAllTrade.Usage.OutputAsInformation();

async ValueTask<object?> FunctionCallMiddleware(AIAgent callingAgent, FunctionInvocationContext context, Func<FunctionInvocationContext, CancellationToken, ValueTask<object?>> next, CancellationToken cancellationToken)
{
    StringBuilder functionCallDetails = new();
    functionCallDetails.Append($"- Tool Call: '{context.Function.Name}' [Agent: {callingAgent.Name}]");
    if (context.Arguments.Count > 0)
    {
        functionCallDetails.Append($" (Args: {string.Join(",", context.Arguments.Select(x => $"[{x.Key} = {x.Value}]"))}");
    }

    Utils.WriteLineDarkGray(functionCallDetails.ToString());

    return await next(context, cancellationToken);
}