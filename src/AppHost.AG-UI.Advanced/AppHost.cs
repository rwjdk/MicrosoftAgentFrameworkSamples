var builder = DistributedApplication.CreateBuilder(args);

IResourceBuilder<ProjectResource> server = builder.AddProject<Projects.AgentUserInteraction_Advanced_Server>("server");

builder.AddProject<Projects.AgentUserInteraction_Advanced_BlazorWasmClient>("blazor-wasm-client")
    .WaitFor(server);

builder.Build().Run();