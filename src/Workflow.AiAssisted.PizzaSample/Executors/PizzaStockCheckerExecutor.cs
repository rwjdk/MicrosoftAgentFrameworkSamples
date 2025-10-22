using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Reflection;
using Shared;
using Workflow.AiAssisted.PizzaSample.Models;

namespace Workflow.AiAssisted.PizzaSample.Executors;

class PizzaStockCheckerExecutor() : ReflectingExecutor<PizzaStockCheckerExecutor>("StockChecker"), IMessageHandler<PizzaOrder, PizzaOrder>
{
    public ValueTask<PizzaOrder> HandleAsync(PizzaOrder message, IWorkflowContext context, CancellationToken cancellationToken)
    {
        foreach (string topping in message.Toppings)
        {
            if (topping == "Mushrooms") //Sample out of stock
            {
                Utils.WriteLineDarkGray($"--- Add out of stock warning: {topping}");
                message.Warnings.Add(WarningType.OutOfIngredient, topping);
            }
            else
            {
                Utils.WriteLineYellow($"- Add {topping} onto Pizza (Reduced stock)");
            }
        }

        return ValueTask.FromResult(message);
    }
}