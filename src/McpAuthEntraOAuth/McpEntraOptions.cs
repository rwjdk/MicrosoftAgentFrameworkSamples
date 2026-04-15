using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace McpAuthEntraOAuth;

public sealed class McpEntraOptions
{
    public const string SectionName = "McpEntra";
    public const string DefaultRequiredScope = "mcp.access";

    [Required]
    public string TenantId { get; init; } = string.Empty;

    [Required]
    public string ClientId { get; init; } = string.Empty;

    public string ApplicationIdUri { get; init; } = string.Empty;

    public string RequiredScope { get; init; } = DefaultRequiredScope;

    public string Authority => $"https://login.microsoftonline.com/{TenantId}/v2.0";

    public string AuthorizationServerIssuer => Authority;

    public string EffectiveApplicationIdUri => string.IsNullOrWhiteSpace(ApplicationIdUri)
        ? $"api://{ClientId}"
        : ApplicationIdUri;

    public string RequiredScopeForChallenges => $"{EffectiveApplicationIdUri.TrimEnd('/')}/{RequiredScope}";

    public string[] ValidAudiences =>
    [
        ClientId,
        EffectiveApplicationIdUri
    ];

    public string[] ValidIssuers =>
    [
        Authority,
        $"https://sts.windows.net/{TenantId}/"
    ];

    public static bool HasRequiredScope(ClaimsPrincipal user, string requiredScope)
    {
        foreach (Claim claim in user.FindAll("scp"))
        {
            string[] scopes = claim.Value.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (scopes.Contains(requiredScope, StringComparer.Ordinal))
            {
                return true;
            }
        }

        return false;
    }
}
