using System.ClientModel;
using Azure.AI.Projects;
using Azure.AI.Projects.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using OpenAI.Responses;
using Shared;

#pragma warning disable OPENAI001
Console.Clear();

Secrets secrets = SecretManager.GetConfiguration();

AIProjectClient client = new Azure.AI.Projects.AIProjectClient(new Uri(secrets.AzureAiFoundryAgentEndpoint), new AzureCliCredential());

string? foundryAgentName = null;
try
{
    ClientResult<AgentVersion> foundryAgent = await client.Agents.CreateAgentVersionAsync( // <-- Reveal that version history is coming
        agentName: "MyNewTypeOfAgent",
        options: new AgentVersionCreationOptions(
            new PromptAgentDefinition("gpt-5-mini")
            {
                Instructions = "You are a nice Agent",
                /* <-- Not working for now, but the fact that this is here indicate either "bad design" or Agents will stay on OpenAI forever
                ReasoningOptions = new ResponseReasoningOptions //
                {
                    ReasoningEffortLevel = ResponseReasoningEffortLevel.Minimal
                }*/
            }
        )
    );
    foundryAgentName = foundryAgent.Value.Name;

    ChatClientAgent agentFrameworkAgent = client.GetAIAgent(foundryAgent);

    AgentRunResponse response = await agentFrameworkAgent.RunAsync("What is the Capital of France");
    Console.WriteLine(response);
}
catch (Exception ex)
{
    Utils.WriteLineRed("Error: " + ex);
}
finally
{
    if (foundryAgentName != null)
    {
        await client.Agents.DeleteAgentAsync(foundryAgentName);
    }
}