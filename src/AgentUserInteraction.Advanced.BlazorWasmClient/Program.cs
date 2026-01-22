using AgentUserInteraction.Advanced.BlazorWasmClient;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.AGUI;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.AI;
using AgentUserInteraction.Advanced.BlazorWasmClient.Pages;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

HttpClient httpClient = new();
string serverRoot = "https://localhost:7219";

ChatClientAgent changeColorAgent = (new AGUIChatClient(httpClient, $"{serverRoot}/clientToolAgent")).AsAIAgent(tools: [AIFunctionFactory.Create(ChangeColor)]);
ChatClientAgent weatherAgent = (new AGUIChatClient(httpClient, $"{serverRoot}/weatherAgent")).AsAIAgent();
ChatClientAgent weatherAgentWithStructuredContent = (new AGUIChatClient(httpClient, $"{serverRoot}/weatherAgentWithStructuredContent")).AsAIAgent();
ChatClientAgent movieAgent = (new AGUIChatClient(httpClient, $"{serverRoot}/movieAgent")).AsAIAgent();

builder.Services.AddSingleton(new AgentCollection(movieAgent, weatherAgent, weatherAgentWithStructuredContent, changeColorAgent));

WebAssemblyHost app = builder.Build();

await app.RunAsync();

static void ChangeColor(string color)
{
    Home.Color = color;
}