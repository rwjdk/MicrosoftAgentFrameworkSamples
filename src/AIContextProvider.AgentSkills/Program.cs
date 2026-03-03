using AgentSkills;
using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI.Chat;
using Shared;

#pragma warning disable MAAI001
Utils.Init("AI Context Provider (AgentSkills)");
AzureOpenAIClient client = Utils.GetAzureOpenAIClient(showRawCall: true);

string skillPath = "TestData\\AgentSkills";

AIAgent agent = client.GetChatClient("gpt-4.1-mini").AsAIAgent(new ChatClientAgentOptions
{
    AIContextProviders = [new FileAgentSkillsProvider(skillPath)],
    ChatOptions = new ChatOptions
    {
        Tools = [AIFunctionFactory.Create(PythonRunner.RunPhytonScript, name: "execute_python")]
    }
}).AsBuilder().Use(Utils.ToolCallingMiddleware).Build();


await Utils.RunChatLoopWithSession(agent);