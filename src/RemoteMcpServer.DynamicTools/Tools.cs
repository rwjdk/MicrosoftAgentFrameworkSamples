using System.ComponentModel;
using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using ModelContextProtocol.Server;

namespace RemoteMcpServer.DynamicTools;

[McpServerToolType]
public class Tools
{
    [McpServerTool(Name = ToolNames.Tool1)]
    public string Tool1()
    {
        return "Tool1Output";
    }

    [McpServerTool(Name = ToolNames.Tool2)]
    public string Tool2()
    {
        return "Tool2Output";
    }

    [McpServerTool(Name = ToolNames.AdminTool1)]
    public string AdminTool1()
    {
        return "AdminTool1Output";
    }
}