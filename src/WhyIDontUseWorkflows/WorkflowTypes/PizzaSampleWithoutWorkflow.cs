using Anthropic.Models.Messages;
using Microsoft.Agents.AI;
using Shared;
using System.Text;
using System.Threading;
using Workflow.AiAssisted.PizzaSample;
using Workflow.AiAssisted.PizzaSample.Models;

namespace WhyIDontUseWorkflows.WorkflowTypes;

public static class PizzaSampleWithoutWorkflow
{
    public static async Task Run()
    {
        Secrets secrets = SecretManager.GetSecrets();
        AgentFactory agentFactory = new(secrets);

        Console.OutputEncoding = Encoding.UTF8;

        const string input = "Make a big Pepperoni Pizza with mushrooms and onions";
        Utils.Yellow("- Parse order");
        AgentResponse<PizzaOrder> orderResponse = await agentFactory.CreateOrderTakerAgent().RunAsync<PizzaOrder>(input);
        var order = orderResponse.Result;

        foreach (string topping in order.Toppings)
        {
            if (topping == "Mushrooms") //Sample out of stock
            {
                Utils.Gray($"--- Add out of stock warning: {topping}");
                order.Warnings.Add(WarningType.OutOfIngredient, topping);
            }
            else
            {
                Utils.Yellow($"- Add {topping} onto Pizza (Reduced stock)");
            }
        }

        if (order.Warnings.Count == 0)
        {
            Utils.Yellow("- Pizza OK 😋");
        }
        else
        {
            Utils.Red("Can't create the pizza in full");

            StringBuilder sb = new();
            foreach (KeyValuePair<WarningType, string> warning in order.Warnings)
            {
                sb.AppendLine($" - {warning}: {warning.Value}");
            }

            AgentResponse response = await agentFactory.CreateWarningToCustomerAgent().RunAsync($"Explain to the use can't we can't for-fill their order do to the following: {sb}");
            Console.WriteLine("Send as email: " + response);
        }
    }
}