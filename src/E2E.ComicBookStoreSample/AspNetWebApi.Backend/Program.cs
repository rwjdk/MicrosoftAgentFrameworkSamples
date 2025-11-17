using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Hosting.AGUI.AspNetCore;
using OpenAI;
using System.ClientModel;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddOpenApi();

//Configuration
string azureOpenAIEndpoint = builder.Configuration["CBS_AZURE_OPEN_AI_ENDPOINT"] ?? throw new ApplicationException("azureOpenAIEndpoint env. variable is missing");
string azureOpenAIKey = builder.Configuration["CBS_AZURE_OPEN_AI_KEY"] ?? throw new ApplicationException("azureOpenAIKey env. variable is missing");
string comicBookGuyModel = builder.Configuration["CBS_COMIC_BOOK_GUY_AGENT_MODEL"] ?? throw new ApplicationException("comic-book-guy-agent-model env. variable is missing");
string assistantModel = builder.Configuration["CBS_ASSISTANT_AGENT_MODEL"] ?? throw new ApplicationException("assistant-agent-model env. variable is missing");

builder.Services.AddSingleton(new AzureOpenAIClient(new Uri(azureOpenAIEndpoint), new ApiKeyCredential(azureOpenAIKey)));
builder.Services.AddAGUI();

builder.Services.AddCors(options =>
{
    options.AddPolicy("Cors",
        policy =>
        {
            policy.WithOrigins("*")
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});

WebApplication app = builder.Build();

AzureOpenAIClient azureOpenAIClient = app.Services.GetRequiredService<AzureOpenAIClient>();

AIAgent comicBookGuyAgent = azureOpenAIClient
    .GetChatClient(comicBookGuyModel)
    .CreateAIAgent(instructions: "You are comic-book-guy from the Simpsons. Do not use Markdown in the answers")
    .AsBuilder()
    .UseOpenTelemetry("ComicBookGuySource", telemetryAgent => telemetryAgent.EnableSensitiveData = true)
    .Build();
;

AIAgent assistantAgent = azureOpenAIClient
    .GetChatClient(assistantModel)
    .CreateAIAgent(instructions: "You are comic-book-guy from the Simpsons sane assistant when he become a bit too much. Do not use Markdown in the answers")
    .AsBuilder()
    .UseOpenTelemetry("AssistantSource", telemetryAgent => telemetryAgent.EnableSensitiveData = true)
    .Build();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapAGUI("/comic-book-guy", comicBookGuyAgent);
app.MapAGUI("/assistant", assistantAgent);

app.UseCors("Cors");

app.UseHttpsRedirection();

app.Run();