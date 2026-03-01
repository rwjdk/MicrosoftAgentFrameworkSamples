using Microsoft.Agents.AI.Workflows;
using Shared;
using Workflow.AiAssisted.PizzaSample.Models;

namespace Workflow.AiAssisted.PizzaSample.Executors;

class PizzaStockCheckerExecutor() : Executor<PizzaOrder, PizzaOrder>("StockChecker")
{
    public override ValueTask<PizzaOrder> HandleAsync(PizzaOrder message, IWorkflowContext context, CancellationToken cancellationToken)
    {
        foreach (string topping in message.Toppings)
        {
            if (topping == "Mushrooms") //Sample out of stock
            {
                Utils.Gray($"--- Add out of stock warning: {topping}");
                message.Warnings.Add(WarningType.OutOfIngredient, topping);
            }
            else
            {
                Utils.Yellow($"- Add {topping} onto Pizza (Reduced stock)");
            }
        }

        return ValueTask.FromResult(message);
    }
}