namespace Workflow.AiAssisted.PizzaSample.Models;

class PizzaOrder
{
    public PizzaSize Size { get; set; }
    public List<string> Toppings { get; set; } = [];
    public Dictionary<WarningType, string> Warnings { get; set; } = [];
}