using McpAuthEntraOAuth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Options;

//WARNING: THIS SAMPLE IS NOT YET WORKING!!!

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
IConfigurationSection entraOptionsSection = builder.Configuration.GetSection(McpEntraOptions.SectionName);
McpEntraOptions configuredEntraOptions = entraOptionsSection.Get<McpEntraOptions>() ?? new McpEntraOptions();
string configuredRequiredScope = entraOptionsSection[nameof(McpEntraOptions.RequiredScope)] ?? McpEntraOptions.DefaultRequiredScope;

builder.Services
    .AddOptions<McpEntraOptions>()
    .Bind(entraOptionsSection)
    .ValidateDataAnnotations()
    .Validate(options => !string.IsNullOrWhiteSpace(options.TenantId), $"{McpEntraOptions.SectionName}:TenantId is required.")
    .Validate(options => !string.IsNullOrWhiteSpace(options.ClientId), $"{McpEntraOptions.SectionName}:ClientId is required.")
    .ValidateOnStart();

builder.Services.AddSingleton<McpProtectedResourceMetadataFactory>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = configuredEntraOptions.Authority;
        options.RequireHttpsMetadata = true;
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuers = configuredEntraOptions.ValidIssuers,
            ValidateAudience = true,
            ValidAudiences = configuredEntraOptions.ValidAudiences,
            NameClaimType = "name",
            RoleClaimType = "roles"
        };
        options.Events = new JwtBearerEvents
        {
            OnChallenge = context =>
            {
                if (!context.Request.Path.StartsWithSegments(McpProtectedResourceMetadataFactory.McpEndpointPath))
                {
                    return Task.CompletedTask;
                }

                context.HandleResponse();

                McpProtectedResourceMetadataFactory metadataFactory = context.HttpContext.RequestServices.GetRequiredService<McpProtectedResourceMetadataFactory>();
                McpEntraOptions settings = context.HttpContext.RequestServices.GetRequiredService<IOptions<McpEntraOptions>>().Value;
                string challenge = metadataFactory.BuildUnauthorizedChallenge(context.Request, settings, context.AuthenticateFailure is not null);

                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.Headers.WWWAuthenticate = challenge;

                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("McpScope", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireAssertion(context => McpEntraOptions.HasRequiredScope(context.User, configuredRequiredScope));
    });
});

builder.Services.AddSingleton<IAuthorizationMiddlewareResultHandler, McpAuthorizationMiddlewareResultHandler>();

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedHost | ForwardedHeaders.XForwardedProto;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

builder.Services.AddMcpServer()
    .WithHttpTransport()
    .WithToolsFromAssembly();

WebApplication app = builder.Build();

app.UseForwardedHeaders();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/.well-known/oauth-protected-resource", (HttpContext context, IOptions<McpEntraOptions> options, McpProtectedResourceMetadataFactory metadataFactory) =>
{
    return Results.Json(metadataFactory.CreateDocument(context.Request, options.Value));
});

app.MapGet("/.well-known/oauth-protected-resource/mcp", (HttpContext context, IOptions<McpEntraOptions> options, McpProtectedResourceMetadataFactory metadataFactory) =>
{
    return Results.Json(metadataFactory.CreateDocument(context.Request, options.Value));
});

app.MapMcp(McpProtectedResourceMetadataFactory.McpEndpointPath)
    .RequireAuthorization("McpScope");

app.Run();
