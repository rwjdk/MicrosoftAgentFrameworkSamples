//YouTube video that cover this sample: https://youtu.be/q-mHdd6iJJo

using Azure.AI.OpenAI;
using DependencyInjection.Components;
using System.ClientModel;
using Microsoft.Agents.AI;
using OpenAI;
using OpenAI.Chat;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

Shared.Secrets secrets = Shared.SecretManager.GetSecrets();

AzureOpenAIClient client = new AzureOpenAIClient(new Uri(secrets.AzureOpenAiEndpoint), new ApiKeyCredential(secrets.AzureOpenAiKey));
builder.Services.AddSingleton(client);

ChatClient chatClient = client.GetChatClient("gpt-4.1");
builder.Services.AddKeyedSingleton("gpt-4.1", chatClient);

ChatClientAgent agent = chatClient.AsAIAgent();
builder.Services.AddKeyedSingleton("gpt-4.1", agent);

WebApplication app = builder.Build();
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAntiforgery();
app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();
app.Run();