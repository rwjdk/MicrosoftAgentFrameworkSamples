using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMcpServer()
    .WithHttpTransport()
    .WithToolsFromAssembly();

AuthenticationBuilder authenticationBuilder = builder.Services.AddAuthentication("MyScheme");
authenticationBuilder.AddScheme<AuthenticationSchemeOptions, McpApiKeyAuthenticationHandler>("MyScheme", _ => { });
builder.Services.AddAuthorization();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();
app.MapMcp("/mcp").RequireAuthorization();

app.UseHttpsRedirection();

app.Run();


class McpApiKeyAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        StringValues sentApiKey = Request.Headers["x-api-key"];
        if (string.IsNullOrWhiteSpace(sentApiKey))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        string apiKey = "MySecretKey"; //todo - store somewhere else

        if (!string.Equals(sentApiKey, apiKey, StringComparison.Ordinal))
        {
            return Task.FromResult(AuthenticateResult.Fail("Invalid MCP API key."));
        }

        ClaimsIdentity identity = new([
            new Claim(ClaimTypes.Name, "MyMcpApiKeyCheck")
        ], "MyScheme");
        ClaimsPrincipal user = new(identity);
        AuthenticationTicket ticket = new(user, "MyScheme");
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
