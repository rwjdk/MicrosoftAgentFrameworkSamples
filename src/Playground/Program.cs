//WARNING: This is a playground area for the creator of the Repo to test and tinker. Nothing in this project is as such educational and might not even execute properly

//Notes
//- Microsoft.Agents.AI.Hosting.AgentCatalog TODO: Guess this is something to be used in AI Foundry

#pragma warning disable OPENAI001
using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI;
using OpenAI.Responses;
using Shared;
using System.ClientModel;
using System.ClientModel.Primitives;
using System.Text;
using System.Text.Json;
using A2A;
using GenerativeAI;
using GenerativeAI.Microsoft;

Console.WriteLine("");
Console.Clear();

Configuration configuration = ConfigurationManager.GetConfiguration();

string apiKey = configuration.GoogleGeminiApiKey;
const string model = GoogleAIModels.Gemini25Pro;

IChatClient clientG = new GenerativeAIChatClient(apiKey, model);

await foreach (ChatResponseUpdate update in clientG.GetStreamingResponseAsync("What is the weather like in Paris", new ChatOptions
               {
                   Tools = [AIFunctionFactory.Create(GetWeather)]
               }))
{
    Console.WriteLine(update);
}


ChatClientAgent agentF = new(clientG, tools: [AIFunctionFactory.Create(GetWeather)]);

AgentRunResponse agentRunResponse = await agentF.RunAsync("What is the weather like in Paris");
Console.WriteLine(agentRunResponse);

await foreach (AgentRunResponseUpdate update in agentF.RunStreamingAsync("What is the weather like in Paris"))
{
    Console.Write(update);
}


AzureOpenAIClient client = new(new Uri(configuration.AzureOpenAiEndpoint), new ApiKeyCredential(configuration.AzureOpenAiKey), new AzureOpenAIClientOptions
{
    //Transport = new HttpClientPipelineTransport(httpClient)
});


AIAgent agent = client
    .GetOpenAIResponseClient("gpt-5-mini")
    .CreateAIAgent();

AgentRunResponse response = await agent.RunAsync("What is the capital of France and how many live there?");
Console.WriteLine(response);

//await AzureOpenAiFoundry.Run(configuration);
//await FileTool.Run(configuration);
//await CodeTool.Run(configuration);
//await ReasoningSummary.Run(configuration);
//await CodexSpecialModels.Run(configuration);
//await SpaceNewsWebSearch.Run(configuration);
//await ResumeConversation.Run(configuration);
//await AzureOpenAiCodex.Run(configuration);

static string GetWeather(string city)
{
    return "It is sunny and 19 degrees today";
}