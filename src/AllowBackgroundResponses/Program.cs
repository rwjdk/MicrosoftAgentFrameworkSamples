//YouTube video that cover this sample: https://youtu.be/6sM6nTk_UBs

using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Shared;
using System.ClientModel;
using OpenAI;

#pragma warning disable OPENAI001

Console.Clear();

Secrets secrets = SecretManager.GetConfiguration();

AzureOpenAIClient client = new(new Uri(secrets.AzureOpenAiEndpoint), new ApiKeyCredential(secrets.AzureOpenAiKey));

AIAgent agent = client
    .GetOpenAIResponseClient("gpt-5")
    .CreateAIAgent();

Utils.WriteLineGreen("SimpleQuestion-BEGIN");
AgentRunResponse response1 = await agent.RunAsync("What is the capital of France?");
Console.WriteLine(response1);
Utils.WriteLineGreen("SimpleQuestion-END");

Console.Clear();

Utils.WriteLineGreen("BigQuestion-BEGIN");
AgentRunResponse response2 = await agent.RunAsync("Write a 1000 word essay on Pigs in Space");
Console.WriteLine(response2);
Utils.WriteLineGreen("BigQuestion-END");

Console.Clear();

Utils.WriteLineGreen("BigQuestion-BACKGROUND-BEGIN");
AgentThread agentThread = agent.GetNewThread();
ChatClientAgentRunOptions options = new ChatClientAgentRunOptions
{
    AllowBackgroundResponses = true
};
AgentRunResponse response3 = await agent.RunAsync("Write a 2000 word essay on Pigs in Space", agentThread, options: options);
Utils.WriteLineGreen("BigQuestion-BACKGROUND-END");

int counter = 0;
while (response3.ContinuationToken is not null)
{
    await Task.Delay(TimeSpan.FromSeconds(2));
    counter++;
    Utils.WriteLineDarkGray($"- Waited: {(counter * 2)} seconds...");

    options.ContinuationToken = response3.ContinuationToken;
    response3 = await agent.RunAsync(agentThread, options);
}

Console.WriteLine(response3.Text);