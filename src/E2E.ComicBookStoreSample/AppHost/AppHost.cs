IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

IResourceBuilder<ProjectResource> backend = builder.AddProject<Projects.AspNetWebApi_Backend>("aspnet-webapi-backend");

builder.AddProject<Projects.BlazorWasm_FrontEnd>("blazor-wasm-frontend")
    .WaitFor(backend);

builder.Build().Run();