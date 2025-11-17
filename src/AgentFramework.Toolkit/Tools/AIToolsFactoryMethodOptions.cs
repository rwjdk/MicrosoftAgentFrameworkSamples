namespace AgentFramework.Toolkit.Tools;

public class AIToolsFactoryMethodOptions
{
    public bool MakeNamesSnakeCase { get; set; } = true;
    public bool IncludeDescriptions { get; set; } = true;
    public bool IncludePublicMethods { get; set; } = true;
    public bool IncludeNonPublicMethods { get; set; } = false;
    public bool IncludePublicStaticMethods { get; set; } = true;
    public bool IncludeNonPublicStaticMethods { get; set; } = false;
    public bool DeclaredOnly { get; set; } = true;
}