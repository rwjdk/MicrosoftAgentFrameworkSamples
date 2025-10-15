using JetBrains.Annotations;

namespace Workflow.AiAssisted.PizzaSample.Models;

[PublicAPI]
class PizzaOrder
{
    public PizzaSize Size { get; set; }
    public List<string> Toppings { get; set; } = [];
    public Dictionary<WarningType, string> Warnings { get; set; } = [];
}