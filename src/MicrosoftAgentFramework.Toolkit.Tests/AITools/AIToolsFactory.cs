using Microsoft.Extensions.AI;
using MicrosoftAgentFramework.Toolkit.AITools;
using MicrosoftAgentFramework.Toolkit.Tests.AITools.SampleTools;

namespace MicrosoftAgentFramework.Toolkit.Tests.AITools;

public class AIToolsFactory
{
    [Fact]
    public void OnlyPublic()
    {
        MethodClassWithTools instance = new();
        IList<AITool> tools = Toolkit.AITools.AIToolsFactory.GetToolsFromMethods(instance);
        Assert.Equal(2, tools.Count);
        string[] toolNames = tools.Select(x => x.Name).ToArray();
        Assert.Contains("public_static_tool", toolNames);
        Assert.Contains("public_tool", toolNames);
    }

    [Fact]
    public void PublicAndPrivate()
    {
        MethodClassWithTools instance = new();
        IList<AITool> tools = Toolkit.AITools.AIToolsFactory.GetToolsFromMethods(instance, new AIToolsFactoryMethodOptions
        {
            IncludeNonPublicStaticMethods = true,
            IncludeNonPublicMethods = true,
        });
        Assert.Equal(4, tools.Count);
        string[] toolNames = tools.Select(x => x.Name).ToArray();
        Assert.Contains("public_static_tool", toolNames);
        Assert.Contains("private_static_tool", toolNames);
        Assert.Contains("public_tool", toolNames);
        Assert.Contains("private_tool", toolNames);
    }
}