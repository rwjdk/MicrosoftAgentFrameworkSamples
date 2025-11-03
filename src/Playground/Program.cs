//WARNING: This is a playground area for the creator of the Repo to test and tinker. Nothing in this project is as such educational and might not even execute properly

//Notes
//- Microsoft.Agents.AI.Hosting.AgentCatalog TODO: Guess this is something to be used in AI Foundry

#pragma warning disable OPENAI001
using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI;
using Shared;
using System.ClientModel;
using System.Text;

Console.WriteLine("");
Console.Clear();

Configuration configuration = ConfigurationManager.GetConfiguration();

AzureOpenAIClient client = new(new Uri(configuration.AzureOpenAiEndpoint), new ApiKeyCredential(configuration.AzureOpenAiKey));

AIAgent agentFast = client
    .GetOpenAIResponseClient("gpt-5")
    .CreateAIAgent();

Utils.WriteLineGreen("AgentFast-BEGIN");
AgentRunResponse response1 = await agentFast.RunAsync("What is the capital of France?");
Console.WriteLine(response1);
Utils.WriteLineGreen("AgentFast-END");

Console.Clear();

AIAgent agentSlow = client
    .GetOpenAIResponseClient(configuration.ChatDeploymentName)
    .CreateAIAgent();

Utils.WriteLineGreen("AgentSlow-BEGIN");
AgentRunResponse response2 = await agentSlow.RunAsync("Write a 2000 word essay on Pigs in Space");
Console.WriteLine(response2);
Utils.WriteLineGreen("AgentSlow-END");

Console.Clear();

Utils.WriteLineGreen("AgentSlow-BACKGROUND-BEGIN");
AgentThread agentThread = agentSlow.GetNewThread();
ChatClientAgentRunOptions options = new ChatClientAgentRunOptions
{
    AllowBackgroundResponses = true
};
AgentRunResponse response3 = await agentSlow.RunAsync("Write a 2000 word essay on Pigs in Space", agentThread, options: options);
Utils.WriteLineGreen("AgentSlow-BACKGROUND-END");

int counter = 0;
while (response3.ContinuationToken is not null)
{
    await Task.Delay(TimeSpan.FromSeconds(2));
    counter++;
    Utils.WriteLineDarkGray($"- Waited: {(counter * 2)} seconds...");

    options.ContinuationToken = response3.ContinuationToken;
    response3 = await agentSlow.RunAsync(agentThread, options);
}

Console.WriteLine(response3.Text);

//await AzureOpenAiFoundry.Run(configuration);
//await FileTool.Run(configuration);
//await CodeTool.Run(configuration);
//await ReasoningSummary.Run(configuration);
//await CodexSpecialModels.Run(configuration);
//await SpaceNewsWebSearch.Run(configuration);
//await ResumeConversation.Run(configuration);
//await AzureOpenAiCodex.Run(configuration);