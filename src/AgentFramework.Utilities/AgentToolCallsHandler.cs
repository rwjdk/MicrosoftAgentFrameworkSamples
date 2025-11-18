using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace AgentFramework.Utilities;

public class AgentToolCallsHandler(Action<AgentToolCallingDetails> toolCallDetails)
{
    public async ValueTask<object?> ToolCallingMiddleware(AIAgent agent, FunctionInvocationContext context, Func<FunctionInvocationContext, CancellationToken, ValueTask<object?>> next, CancellationToken cancellationToken)
    {
        object? result = await next(context, cancellationToken);
        toolCallDetails.Invoke(new AgentToolCallingDetails
        {
            Context = context
        });
        return result;
    }
}