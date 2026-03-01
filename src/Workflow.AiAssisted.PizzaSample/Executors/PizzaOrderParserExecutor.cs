using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Shared;
using Workflow.AiAssisted.PizzaSample.Models;

namespace Workflow.AiAssisted.PizzaSample.Executors;

class PizzaOrderParserExecutor(ChatClientAgent agent) : Executor<string, PizzaOrder>("OrderParser")
{
    public override async ValueTask<PizzaOrder> HandleAsync(string message, IWorkflowContext context, CancellationToken cancellationToken)
    {
        Utils.Yellow("- Parse order");
        AgentResponse<PizzaOrder> orderResponse = await agent.RunAsync<PizzaOrder>(message, cancellationToken: cancellationToken);
        return orderResponse.Result;
    }
}