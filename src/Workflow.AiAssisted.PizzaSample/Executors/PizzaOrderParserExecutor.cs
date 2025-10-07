using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Reflection;
using Shared;
using Workflow.AiAssisted.PizzaSample.Models;

namespace Workflow.AiAssisted.PizzaSample.Executors;

class PizzaOrderParserExecutor(AIAgent agent) : ReflectingExecutor<PizzaOrderParserExecutor>("OrderParser"), IMessageHandler<string, PizzaOrder>
{
    public async ValueTask<PizzaOrder> HandleAsync(string message, IWorkflowContext context)
    {
        Utils.WriteLineYellow("- Parse order");
        AgentRunResponse orderResponse = await agent.RunAsync(message);
        PizzaOrder pizzaOrder = orderResponse.Deserialize<PizzaOrder>(new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
            Converters = { new JsonStringEnumConverter() }
        });
        return pizzaOrder;
    }
}