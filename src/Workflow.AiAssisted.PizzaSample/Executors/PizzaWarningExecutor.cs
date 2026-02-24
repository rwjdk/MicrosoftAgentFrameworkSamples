using System.Text;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Shared;
using Workflow.AiAssisted.PizzaSample.Models;

namespace Workflow.AiAssisted.PizzaSample.Executors;

class PizzaWarningExecutor(ChatClientAgent warningToCustomerAgent) : Executor<PizzaOrder>("PizzaWarning")
{
    public override async ValueTask HandleAsync(PizzaOrder message, IWorkflowContext context, CancellationToken cancellationToken)
    {
        Utils.WriteLineRed("Can't create the pizza in full");

        StringBuilder sb = new();
        foreach (KeyValuePair<WarningType, string> warning in message.Warnings)
        {
            sb.AppendLine($" - {warning}: {warning.Value}");
        }

        AgentResponse response = await warningToCustomerAgent.RunAsync($"Explain to the use can't we can't for-fill their order do to the following: {sb}", cancellationToken: cancellationToken);
        Console.WriteLine("Send as email: " + response);
    }
}