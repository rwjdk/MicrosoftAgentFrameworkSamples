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

using var handler = new CustomClientHttpHandler();
using var httpClient = new HttpClient(handler);
Console.WriteLine("");
Console.Clear();

Configuration configuration = ConfigurationManager.GetConfiguration();

AzureOpenAIClient client = new(new Uri(configuration.AzureOpenAiEndpoint), new ApiKeyCredential(configuration.AzureOpenAiKey), new AzureOpenAIClientOptions
{
    Transport = new HttpClientPipelineTransport(httpClient)
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

class CustomClientHttpHandler() : HttpClientHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        string requestString = await request.Content?.ReadAsStringAsync(cancellationToken)!;
        Utils.WriteLineGreen($"Raw Request ({request.RequestUri})");
        Utils.WriteLineDarkGray(MakePretty(requestString));
        Utils.Separator();
        var response = await base.SendAsync(request, cancellationToken);

        string responseString = await response.Content.ReadAsStringAsync(cancellationToken);
        Utils.WriteLineGreen("Raw Response");
        Utils.WriteLineDarkGray(MakePretty(responseString));
        Utils.Separator();
        return response;
    }

    private string MakePretty(string input)
    {
        var jsonElement = JsonSerializer.Deserialize<JsonElement>(input);
        return JsonSerializer.Serialize(jsonElement, new JsonSerializerOptions { WriteIndented = true });
    }
}