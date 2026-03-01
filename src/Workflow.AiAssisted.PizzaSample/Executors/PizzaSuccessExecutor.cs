using Microsoft.Agents.AI.Workflows;
using Shared;
using Workflow.AiAssisted.PizzaSample.Models;

namespace Workflow.AiAssisted.PizzaSample.Executors;

class PizzaSuccessExecutor() : Executor<PizzaOrder>("PizzaSuccess")
{
    public override ValueTask HandleAsync(PizzaOrder message, IWorkflowContext context, CancellationToken cancellationToken)
    {
        Utils.Yellow("- Pizza OK 😋");
        return ValueTask.CompletedTask;
    }
}