using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Reflection;
using Shared;
using Workflow.AiAssisted.PizzaSample.Models;

namespace Workflow.AiAssisted.PizzaSample.Executors;

class PizzaSuccessExecutor() : ReflectingExecutor<PizzaSuccessExecutor>("PizzaSuccess"), IMessageHandler<PizzaOrder>
{
    public ValueTask HandleAsync(PizzaOrder message, IWorkflowContext context, CancellationToken cancellationToken)
    {
        Utils.WriteLineYellow("- Pizza OK 😋");
        return ValueTask.CompletedTask;
    }
}