//WARNING: This is a playground area for the creator of the Repo to test and tinker. Nothing in this project is as such educational and might not even execute properly
#pragma warning disable OPENAI002
using Azure;
using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using OpenAI.Chat;
using OpenAI.Responses;
using Playground.Tools;
using Shared;
#pragma warning disable OPENAI001
#pragma warning disable MEAI001

Utils.Init("Playground");

Secrets secrets = SecretsManager.GetSecrets();

AgentBuilderTool agentBuilderTool = new AgentBuilderTool();

AzureOpenAIClient client = ClientHelper.GetAzureOpenAIClient();

AIAgent agent = client
    .GetResponsesClient()
    .AsAIAgent(
        model: "gpt-5"
        //tools: [AIFunctionFactory.Create(agentBuilderTool.RunSubAgent, "call_sub_agent")]
        );

AgentSession session = await agent.CreateSessionAsync();

while (true)
{
    Console.Write("> ");
    string? input = Console.ReadLine();
    if (!string.IsNullOrWhiteSpace(input))
    {
        ChatClientAgentRunOptions options = new()
        {
            AllowBackgroundResponses = true,
            ChatOptions = new ChatOptions
            {
            }
        };
        AgentResponse response = await agent.RunAsync(input, session, options: options);
        int counter = 0;
        while (response.ContinuationToken is not null)
        {
            await Task.Delay(TimeSpan.FromSeconds(2));
            counter++;
            Utils.Gray($"- Waited: {(counter * 2)} seconds...");
            options.ContinuationToken = response.ContinuationToken;
            response = await agent.RunAsync(session, options);
        }
        Console.WriteLine(response);
    }

    Utils.Separator();

}
