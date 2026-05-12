using System.ComponentModel;
using System.Security.Claims;
using ModelContextProtocol.Server;

namespace McpServerWithOAuth.Tools;

[McpServerToolType]
public class SomeTools(IHttpContextAccessor httpContextAccessor)
{
    [McpServerTool(Name = "who_am_i")]
    public string WhoAmI()
    {
        ClaimsPrincipal? user = httpContextAccessor.HttpContext?.User;
        string? email = user?.FindFirstValue("preferred_username");
        return email ?? "Unknown";
    }

    [McpServerTool(Name = "tool_1"), Description("Tool 1")]
    public string PublicTool1(string message)
    {
        return $"PublicTool1 received: {message}";
    }
    
    [McpServerTool(Name = "tool_2"), Description("Tool 2")]
    public string PublicTool2(string message)
    {
        return $"PublicTool2 received: {message}";
    }
}
