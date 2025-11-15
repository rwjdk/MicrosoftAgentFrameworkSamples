using System.Collections;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using BlazorWasm.FrontEnd;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.AGUI;
using Microsoft.Extensions.AI;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

string comicBookAgentUrl = builder.Configuration["comicBookAgentUrl"] ?? throw new Exception("comicBookAgentUrl not defined");
string assistantAgentUrl = builder.Configuration["assistantAgentUrl"] ?? throw new Exception("assistantAgentUrl not defined");

HttpClient httpClient = new();
ChatClientAgent comicBookGuyAgent = new AGUIChatClient(httpClient, comicBookAgentUrl).CreateAIAgent();
ChatClientAgent assistantAgent = new AGUIChatClient(httpClient, assistantAgentUrl).CreateAIAgent();

builder.Services.AddKeyedSingleton("comic-book-guy-agent", comicBookGuyAgent);
builder.Services.AddKeyedSingleton("assistant-agent", assistantAgent);

await builder.Build().RunAsync();