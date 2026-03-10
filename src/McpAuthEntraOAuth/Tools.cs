using System.ComponentModel;
using ModelContextProtocol.Server;

namespace McpAuthEntraOAuth;

[McpServerToolType]
public class Tools
{
    [McpServerTool(Name = "get_the_secret_word", ReadOnly = true)]
    [Description("Get the Top Secret Word")]
    public string GetTheSecretWord()
    {
        return "BananaCake";
    }
}