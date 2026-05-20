//WARNING: This is a playground area for the creator of the Repo to test and tinker. Nothing in this project is as such educational and might not even execute properly
#pragma warning disable OPENAI002
using Azure.AI.OpenAI;
using Azure.AI.Projects;
using Azure.AI.Projects.Agents;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Hyperlight;
using Microsoft.Extensions.AI;
using OpenAI.Chat;
using OpenAI.Responses;
using Shared;
using System.ClientModel;
using Microsoft.Agents.AI.Foundry;

Utils.Init("Playground");

Secrets secrets = SecretsManager.GetSecrets();

#pragma warning disable OPENAI001
AIProjectClient client = new AIProjectClient(new Uri(secrets.AzureAiFoundryAgentEndpoint), new AzureCliCredential());

string model = "gpt-5.4-mini";
string myAgentName = "myAgent";

AIFunction tool = AIFunctionFactory.Create(GetWeather, "get_weather");
client.AgentAdministrationClient.CreateAgentVersion(myAgentName, new ProjectsAgentVersionCreationOptions(
    new DeclarativeAgentDefinition(model)
    {
        Instructions = "You are a nice AI",
        Tools = { tool.AsOpenAIResponseTool() },
        ReasoningOptions = new ResponseReasoningOptions
        {
            ReasoningEffortLevel = new ResponseReasoningEffortLevel("low")
        }
    }));

FoundryAgent agent = client.AsAIAgent(myAgentName, tools: [tool]);

AgentResponse response = await agent.RunAsync("What is the weather like in Paris?");
Console.WriteLine(response);

static string GetWeather(string city)
{
    return "It is Sunny and 19 Degrees";
}