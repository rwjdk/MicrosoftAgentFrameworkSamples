namespace McpAuthEntraOAuth;

public sealed class McpProtectedResourceMetadataFactory
{
    public const string McpEndpointPath = "/mcp";

    public object CreateDocument(HttpRequest request, McpEntraOptions options)
    {
        return new
        {
            resource = GetResourceUrl(request),
            authorization_servers = new[] { options.AuthorizationServerIssuer },
            scopes_supported = new[] { options.RequiredScopeForChallenges },
            bearer_methods_supported = new[] { "header" }
        };
    }

    public string BuildUnauthorizedChallenge(HttpRequest request, McpEntraOptions options, bool includeInvalidTokenError)
    {
        List<string> parts = new List<string>
        {
            $"resource_metadata=\"{GetPathMetadataUrl(request)}\"",
            $"scope=\"{options.RequiredScopeForChallenges}\""
        };

        if (includeInvalidTokenError)
        {
            parts.Add("error=\"invalid_token\"");
        }

        return $"Bearer {string.Join(", ", parts)}";
    }

    public string BuildInsufficientScopeChallenge(HttpRequest request, McpEntraOptions options)
    {
        return $"Bearer resource_metadata=\"{GetPathMetadataUrl(request)}\", scope=\"{options.RequiredScopeForChallenges}\", error=\"insufficient_scope\"";
    }

    private static string GetResourceUrl(HttpRequest request)
    {
        return $"{request.Scheme}://{request.Host}{request.PathBase}{McpEndpointPath}";
    }

    private static string GetPathMetadataUrl(HttpRequest request)
    {
        return $"{request.Scheme}://{request.Host}{request.PathBase}/.well-known/oauth-protected-resource{McpEndpointPath}";
    }
}
