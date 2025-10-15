//YouTube video that cover this sample
//- Basic: https://youtu.be/gJTodKpv8Ik
//- Advanced: https://youtu.be/dCtojrK8bKk
//- MCP: https://youtu.be/Y5IKdt9vdJM

using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI;
using Shared;
using System.ClientModel;
using ToolCalling.Basics;
using ChatMessage = Microsoft.Extensions.AI.ChatMessage;

Configuration configuration = ConfigurationManager.GetConfiguration();

AzureOpenAIClient client = new(new Uri(configuration.AzureOpenAiEndpoint), new ApiKeyCredential(configuration.AzureOpenAiKey));

ChatClientAgent agent = client
    .GetChatClient(configuration.ChatDeploymentName)
    .CreateAIAgent(
        instructions: "You are a Time Expert",
        tools:
        [
            AIFunctionFactory.Create(Tools.CurrentDataAndTime, "current_date_and_time"),
            AIFunctionFactory.Create(Tools.CurrentTimezone, "current_timezone")
        ]
    );

AgentThread thread = agent.GetNewThread();

while (true)
{
    Console.Write("> ");
    string? input = Console.ReadLine();
    ChatMessage message = new(ChatRole.User, input);
    AgentRunResponse response = await agent.RunAsync(message, thread);
    Console.WriteLine(response);

    Utils.Separator();
}