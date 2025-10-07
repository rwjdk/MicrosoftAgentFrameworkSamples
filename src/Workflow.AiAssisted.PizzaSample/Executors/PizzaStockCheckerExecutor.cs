using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Reflection;
using Shared;
using Workflow.AiAssisted.PizzaSample.Models;

namespace Workflow.AiAssisted.PizzaSample.Executors;

class PizzaStockCheckerExecutor() : ReflectingExecutor<PizzaStockCheckerExecutor>("StockChecker"), IMessageHandler<PizzaOrder, PizzaOrder>
{
    public async ValueTask<PizzaOrder> HandleAsync(PizzaOrder message, IWorkflowContext context)
    {
        foreach (string topping in message.Toppings)
        {
            if (topping == "Mushrooms") //Sample out of stock
            {
                Utils.WriteLineInformation($"--- Add out of stock warning: {topping}");
                message.Warnings.Add(WarningType.OutOfIngredient, topping);
            }
            else
            {
                Utils.WriteLineYellow($"- Add {topping} onto Pizza (Reduced stock)");
            }
        }

        return message;
    }
}