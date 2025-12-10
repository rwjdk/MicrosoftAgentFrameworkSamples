using Azure.AI.Projects;
using Azure.AI.Projects.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI.Containers;
using OpenAI.Responses;
using Shared;
using System.ClientModel;

#pragma warning disable OPENAI001

Console.Clear();

Configuration configuration = ConfigurationManager.GetConfiguration();

AIProjectClient client = new AIProjectClient(new Uri(configuration.AzureAiFoundryAgentEndpoint), new AzureCliCredential());

string modelDeploymentName = "gpt-4.1-mini";
string myAgentName = "myAgent4";
string myInstructions = "You are a nice AI";

//Step 0 (Optional): Ensure Model for Agent is Deployed
try
{
    await client.Deployments.GetDeploymentAsync(modelDeploymentName);
}
catch (ClientResultException e)
{
    if (e.Status == 404)
    {
        Console.WriteLine($"Model Deployment '{modelDeploymentName}' was not found");
        return;
    }
    else
    {
        throw;
    }
}

//Step 1: Create/Update Agent if it does not exist
try
{
    ClientResult<AgentRecord> clientResult = await client.Agents.GetAgentAsync(agentName: myAgentName);

    //Let's ensure Agent is as we have defined by making a new version (if definition is the same nothing will happen)
    //await CreateAgent(myInstructions);
}
catch (ClientResultException e)
{
    if (e.Status == 404)
    {
        Console.WriteLine("Agent not found: Creating it");
        await CreateAgent(myInstructions);
    }
    else
    {
        throw;
    }
}

ChatClientAgent agentByName = client.GetAIAgent(name: myAgentName);

AgentRunResponse response = await agentByName.RunAsync("Hi there");
Console.WriteLine(response);

response = await agentByName.RunAsync("What options do the AddCardAsync method in 'TrelloDotNet' (use tools)");
Console.WriteLine(response);

response = await agentByName.RunAsync("What is 23434343*3434343/2323232 (use tools to calculate)");
Console.WriteLine(response);

response = await agentByName.RunAsync("Make a jpg image with graph listing population of the top 10 US States in year 2000");
await GetAndLaunchCodeInterpreterGeneratedFile(response, client);

response = await agentByName.RunAsync("What is the biggest news story today?");
Console.WriteLine(response);

//Let's make a V2 with new instructions
await CreateAgent("Speak like a pirate");

ChatClientAgent agentV2 = client.GetAIAgent(name: myAgentName);
response = await agentV2.RunAsync("Hi there");
Console.WriteLine(response);

AgentVersion agentV1 = (await client.Agents.GetAgentVersionAsync(myAgentName, "1")).Value;
ChatClientAgent agentByVersion = client.GetAIAgent(agentV1);

response = await agentByVersion.RunAsync("Hi Agent 1");
Console.WriteLine(response);

return;

async Task CreateAgent(string instructions)
{
    await client.Agents.CreateAgentVersionAsync(
        agentName: myAgentName,
        options: new AgentVersionCreationOptions(
            new PromptAgentDefinition(modelDeploymentName)
            {
                Tools =
                {
                    new CodeInterpreterTool(new CodeInterpreterToolContainer(new AutomaticCodeInterpreterToolContainerConfiguration())),
                    new WebSearchTool(),

                    //MCP Tools does work, but if add it the Portal GUI claim that it the tool is wrongly configured and you can't save more versions :-(
                    //new McpTool("TrelloDotNetToolAssistant", new Uri("https://trellodotnetassistantbackend.azurewebsites.net/runtime/webhooks/mcp?code=Tools"))
                    //{
                    //    ToolCallApprovalPolicy = new McpToolCallApprovalPolicy(new GlobalMcpToolCallApprovalPolicy("never"))
                    //},
                },
                Instructions = instructions,

                //NB: Reasoning Effort is buggy at the moment in the portal :-(
                //ReasoningOptions = new ResponseReasoningOptions
                //{
                //    ReasoningEffortLevel = ResponseReasoningEffortLevel.Low
                //}
            }
        )
    );
}

async Task GetAndLaunchCodeInterpreterGeneratedFile(AgentRunResponse agentRunResponse, AIProjectClient aiProjectClient)
{
    foreach (ChatMessage message in agentRunResponse.Messages)
    {
        foreach (AIContent content in message.Contents)
        {
            foreach (AIAnnotation annotation in content.Annotations ?? [])
            {
                if (annotation.RawRepresentation is ContainerFileCitationMessageAnnotation containerFileCitation)
                {
                    ContainerClient containerClient = aiProjectClient.OpenAI.GetContainerClient();
                    ClientResult<BinaryData> fileContent = await containerClient.DownloadContainerFileAsync(containerFileCitation.ContainerId, containerFileCitation.FileId);
                    string path = Path.Combine(Path.GetTempPath(), containerFileCitation.Filename);
                    await File.WriteAllBytesAsync(path, fileContent.Value.ToArray());
                    await Task.Factory.StartNew(() =>
                    {
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = path,
                            UseShellExecute = true
                        });
                    });
                }
            }
        }
    }
}