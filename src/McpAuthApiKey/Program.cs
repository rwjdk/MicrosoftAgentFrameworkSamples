var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMcpServer()
    .WithHttpTransport()
    .WithToolsFromAssembly();

var app = builder.Build();

const string expectedApiKey = "MySecretKey"; // TODO: move to config/secret storage.

app.MapMcp("/mcp").AddEndpointFilter(async (context, next) =>
{
    if (!string.Equals(context.HttpContext.Request.Headers["x-api-key"], expectedApiKey, StringComparison.Ordinal))
    {
        return Results.Unauthorized();
    }

    return await next(context);
});

app.UseHttpsRedirection();

app.Run();
