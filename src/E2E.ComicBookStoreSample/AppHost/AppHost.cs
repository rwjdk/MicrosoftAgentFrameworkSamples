IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);


var azureOpenAIEndpoint = builder.AddParameter("AzureOpenAiEndpoint", secret: true);

var azureOpenAIKey = builder.AddParameter("AzureOpenAiKey", secret: true);

IResourceBuilder<ProjectResource> backend = builder
    .AddProject<Projects.AspNetWebApi_Backend>("aspnet-webapi-backend")
    .WithEnvironment("CBS_AZURE_OPEN_AI_ENDPOINT", azureOpenAIEndpoint)
    .WithEnvironment("CBS_AZURE_OPEN_AI_KEY", azureOpenAIKey)
    .WithEnvironment("CBS_COMIC_BOOK_GUY_AGENT_MODEL", "gpt-4.1-mini")
    .WithEnvironment("CBS_ASSISTANT_AGENT_MODEL", "gpt-4.1-mini");

builder.AddProject<Projects.BlazorWasm_FrontEnd>("blazor-wasm-frontend")
    .WaitFor(backend);

builder.Build().Run();