#pragma warning disable MEAI001
using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI;
using Shared;
using System.ClientModel;
using System.Reflection;
using System.Text;
using ToolCalling.Advanced.Tools;
using ChatMessage = Microsoft.Extensions.AI.ChatMessage;

Configuration configuration = ConfigurationManager.GetConfiguration();

AzureOpenAIClient client = new AzureOpenAIClient(new Uri(configuration.Endpoint), new ApiKeyCredential(configuration.Key));

//Get tools via reflection
FileSystemTools target = new FileSystemTools();
MethodInfo[] methods = typeof(FileSystemTools).GetMethods(BindingFlags.Public | BindingFlags.Instance);
List<AITool> listOfTools = methods.Select(x => AIFunctionFactory.Create(x, target)).Cast<AITool>().ToList();

//Approval Tools
listOfTools.Add(new ApprovalRequiredAIFunction(AIFunctionFactory.Create(DangerousTools.SomethingDangerous)));

AIAgent agent = client
    .GetChatClient(configuration.ChatDeploymentName)
    .CreateAIAgent(
        instructions: "You are a File Expert. When working with files you need to provide the full path; not just the filename",
        tools: listOfTools
    )
    .AsBuilder()
    .Use(FunctionCallMiddleware) //Middleware
    .Build();

AgentThread thread = agent.GetNewThread();

while (true)
{
    Console.Write("> ");
    string? input = Console.ReadLine();
    ChatMessage message = new ChatMessage(ChatRole.User, input);
    AgentRunResponse response = await agent.RunAsync(message, thread);
    List<UserInputRequestContent> userInputRequests = response.UserInputRequests.ToList();
    while (userInputRequests.Count > 0)
    {
        List<ChatMessage> userInputResponses = userInputRequests
            .OfType<FunctionApprovalRequestContent>()
            .Select(functionApprovalRequest =>
            {
                Console.WriteLine($"The agent would like to invoke the following function, please reply Y to approve: Name {functionApprovalRequest.FunctionCall.Name}");
                return new ChatMessage(ChatRole.User, [functionApprovalRequest.CreateResponse(Console.ReadLine()?.Equals("Y", StringComparison.OrdinalIgnoreCase) ?? false)]);
            })
            .ToList();

        response = await agent.RunAsync(userInputResponses, thread);
        userInputRequests = response.UserInputRequests.ToList();
    }

    Console.WriteLine(response);

    Utils.Separator();
}

async ValueTask<object?> FunctionCallMiddleware(AIAgent callingAgent, FunctionInvocationContext context, Func<FunctionInvocationContext, CancellationToken, ValueTask<object?>> next, CancellationToken cancellationToken)
{
    StringBuilder functionCallDetails = new();
    functionCallDetails.Append($"- Tool Call: '{context.Function.Name}'");
    if (context.Arguments.Count > 0)
    {
        functionCallDetails.Append($" (Args: {string.Join(",", context.Arguments.Select(x => $"[{x.Key} = {x.Value}]"))}");
    }

    Utils.WriteLineInformation(functionCallDetails.ToString());

    return await next(context, cancellationToken);
}