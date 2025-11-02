namespace MicrosoftAgentFramework.Toolkit.AITools;

public class AiToolDetailsAttribute(string name, string? description = null) : Attribute
{
    public string Name { get; } = name;
    public string? Description { get; } = description;
}