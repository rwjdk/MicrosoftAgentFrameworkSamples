using ModelContextProtocol.AspNetCore.Authentication;
using ModelContextProtocol.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
string authority = builder.Configuration["Authority"]!;
string audience = builder.Configuration["Audience"]!;
string scope = builder.Configuration["Scope"]!;

builder.Services.AddHttpContextAccessor();
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = McpAuthenticationDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.Authority = authority;
    options.Audience = audience;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        ValidAudience = audience,
        ValidIssuer = authority
    };
})
.AddMcp(options =>
{
    options.ResourceMetadata = new ProtectedResourceMetadata
    {
        AuthorizationServers = [authority],
        ScopesSupported = [scope]
    };
});

builder.Services.AddAuthorization();
builder.Services.AddMcpServer().WithHttpTransport().WithToolsFromAssembly();
WebApplication app = builder.Build();
app.UseAuthentication();
app.UseAuthorization();
app.MapMcp().RequireAuthorization();
app.Run();
