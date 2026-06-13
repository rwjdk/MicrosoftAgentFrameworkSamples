using System.ComponentModel;
using ModelContextProtocol.Server;

namespace RemoteMcpServer.DynamicTexts;

[McpServerToolType]
public class Tools
{
    [McpServerTool(Name = "Tool1")]
    [Description("Fixed Description 1")]
    public string Tool1([Description("Fixed Param 1 Description")] string param1)
    {
        return $"Tool1Output ({param1} was input)";
    }

    [McpServerTool(Name = "Tool2")]
    [Description("Fixed Description 2")]
    public string Tool2()
    {
        return "Tool2Output";
    }
}