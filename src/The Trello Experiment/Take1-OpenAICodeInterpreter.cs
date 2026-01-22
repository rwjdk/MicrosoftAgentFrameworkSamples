using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI;
using OpenAI.Responses;

#pragma warning disable OPENAI001

namespace The_Trello_Experiment;

public static class Take1OpenAICodeInterpreter
{
    public static async Task Run(AzureOpenAIClient azureOpenAIClient, string trelloApiKey, string trelloToken)
    {
        ChatClientAgent agent = azureOpenAIClient
            .GetResponsesClient("gpt-5-mini")
            .AsAIAgent(
                instructions: $"""
                               You are a Trello Expert with access to the Trello and the API

                               Here is the credentials: 
                               - API Key: {trelloApiKey}
                               - Token: {trelloToken}

                               Use the 'code_interpreter' tool to talk via the API to Trello
                               """,
                tools: [new HostedCodeInterpreterTool()]
            );

        AgentResponse response = await agent.RunAsync("What Trello Boards do I have?");
        Console.WriteLine(response);
    }
}