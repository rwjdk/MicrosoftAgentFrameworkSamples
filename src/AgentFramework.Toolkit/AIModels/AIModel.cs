namespace AgentFramework.Toolkit.AIModels;

public abstract class AIModel(string modelName)
{
    public string ModelName { get; } = modelName;
    public double? Temperature { get; set; }
}