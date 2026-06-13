using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using System.Text.Json;
using System.Text.Json.Nodes;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

string filename = "dynamicTexts.json";
string targetFile = Path.Combine(Path.GetTempPath(), filename);
if (!File.Exists(targetFile))
{
    File.Copy(filename, targetFile);
}

builder.Services.AddMcpServer()
    .WithHttpTransport()
    .WithToolsFromAssembly()
    .WithTools([
        McpServerTool.Create(ExtraTool, new McpServerToolCreateOptions
        {
            Name = "Extra_Tool",
            Description = "The Extra Tool's description"
        })
    ]).WithRequestFilters(filterBuilder =>
    {
        filterBuilder.AddListToolsFilter(next => async (request, token) =>
        {
            string json = File.ReadAllText(targetFile);
            ToolMetadata[] toolMetadata = JsonSerializer.Deserialize<ToolMetadata[]>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            })!;
            ListToolsResult result = await next(request, token);
            foreach (Tool tool in result.Tools)
            {
                ToolMetadata? metadata = toolMetadata.FirstOrDefault(x => x.Name == tool.Name);
                if (metadata != null)
                {
                    tool.Description = metadata.Description;
                    if (metadata.Parameters != null)
                    {
                        JsonNode node = JsonNode.Parse(tool.InputSchema.GetRawText())!;
                        JsonObject properties = (JsonObject)node["properties"]!;
                        foreach (ToolMetadataParam parameter in metadata.Parameters)
                        {
                            if (properties?[parameter.Id] is JsonObject paramJsonObject)
                            {
                                paramJsonObject["description"] = parameter.Description;
                            }
                        }
                        string updatedJson = node.ToJsonString();
                        tool.InputSchema = JsonDocument.Parse(updatedJson).RootElement;
                    }
                }
            }
            return result;
        });
    });
    

WebApplication app = builder.Build();

app.MapMcp("mcp");

app.Run();

static string ExtraTool()
{
    return "Extra Tool output";
}

record ToolMetadata(string Name, string Description, ToolMetadataParam[]? Parameters);
record ToolMetadataParam(string Id, string Description);