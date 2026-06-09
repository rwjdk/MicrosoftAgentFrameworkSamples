using Microsoft.Extensions.Primitives;
using ModelContextProtocol.Protocol;
using RemoteMcpServer.DynamicTools;


string normalApiKey = "normal_password";
string adminApiKey = "admin_password";

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpContextAccessor();

builder.Services.AddMcpServer()
    .WithHttpTransport()
    .WithToolsFromAssembly()
    .WithRequestFilters(filterBuilder =>
    {
        filterBuilder.AddListToolsFilter(next => async (request, token) =>
        {
            ListToolsResult result = await next(request, token);

            IHttpContextAccessor contextAccessor = request.Services!.GetRequiredService<IHttpContextAccessor>();

            StringValues? givenApiKey = contextAccessor.HttpContext?.Request.Headers["x-api-key"];
            if (givenApiKey.HasValue && givenApiKey.Value == adminApiKey)
            {
                //Admin logged in: Return all Tools
                return result;
            }
            else
            {
                //Non Admin. Only return tools that are non-admin
                return new ListToolsResult
                {
                    Tools = result.Tools.Where(x => !ToolNames.AdminOnlyTools.Contains(x.Name)).ToList()
                };
            }
        });

        filterBuilder.AddCallToolFilter(next => async (request, token) =>
        {
            if (ToolNames.AdminOnlyTools.Contains(request.MatchedPrimitive!.Id))
            {
                //Admin-only tool call. Let's check that user have access to it (by being an admin)
                IHttpContextAccessor contextAccessor = request.Services!.GetRequiredService<IHttpContextAccessor>();
                StringValues? givenApiKey = contextAccessor.HttpContext?.Request.Headers["x-api-key"];
                bool isAdmin = givenApiKey.HasValue && givenApiKey.Value == adminApiKey;
                if (!isAdmin)
                {
                    throw new UnauthorizedAccessException($"User is not allowed to call tool '{request.Params.Name}'.");
                }
            }

            //Normal tool (or passed Admin check)
            return await next(request, token);
        });
    });




WebApplication app = builder.Build();

app.MapMcp("/mcp").AddEndpointFilter(async (context, next) =>
{
    string[] validKeys = [normalApiKey, adminApiKey];

    string receivedApiKey = context.HttpContext.Request.Headers["x-api-key"].ToString();
    if (!validKeys.Contains(receivedApiKey, StringComparer.InvariantCultureIgnoreCase))
    {
        return Results.Unauthorized();
    }
    return await next(context);
});

app.UseHttpsRedirection();

app.Run();