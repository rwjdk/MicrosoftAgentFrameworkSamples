using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Reflection;
using Shared;
using Workflow.AiAssisted.PizzaSample.Models;

namespace Workflow.AiAssisted.PizzaSample.Executors;

class PizzaOrderParserExecutor(ChatClientAgent agent) : ReflectingExecutor<PizzaOrderParserExecutor>("OrderParser"), IMessageHandler<string, PizzaOrder>
{
    public async ValueTask<PizzaOrder> HandleAsync(string message, IWorkflowContext context, CancellationToken cancellationToken)
    {
        Utils.WriteLineYellow("- Parse order");
        ChatClientAgentRunResponse<PizzaOrder> orderResponse = await agent.RunAsync<PizzaOrder>(message, cancellationToken: cancellationToken);
        return orderResponse.Result;
    }
}