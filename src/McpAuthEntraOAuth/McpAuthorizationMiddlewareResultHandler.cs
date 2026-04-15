using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.Extensions.Options;

namespace McpAuthEntraOAuth;

public sealed class McpAuthorizationMiddlewareResultHandler : IAuthorizationMiddlewareResultHandler
{
    private readonly AuthorizationMiddlewareResultHandler _defaultHandler = new();

    public async Task HandleAsync(RequestDelegate next, HttpContext context, AuthorizationPolicy policy, PolicyAuthorizationResult authorizeResult)
    {
        if (authorizeResult.Forbidden && context.Request.Path.StartsWithSegments(McpProtectedResourceMetadataFactory.McpEndpointPath))
        {
            McpProtectedResourceMetadataFactory metadataFactory = context.RequestServices.GetRequiredService<McpProtectedResourceMetadataFactory>();
            McpEntraOptions options = context.RequestServices.GetRequiredService<IOptions<McpEntraOptions>>().Value;

            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            context.Response.Headers.WWWAuthenticate = metadataFactory.BuildInsufficientScopeChallenge(context.Request, options);
            return;
        }

        await _defaultHandler.HandleAsync(next, context, policy, authorizeResult);
    }
}
