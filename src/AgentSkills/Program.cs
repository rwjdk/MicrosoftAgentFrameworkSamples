using AgentFrameworkToolkit.AzureOpenAI;
using AgentFrameworkToolkit.OpenAI;
using AgentSkills;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Shared;
using System.Text;
using AgentSkillsDotNet;

Console.Clear();

Secrets secrets = SecretManager.GetSecrets();

AzureOpenAIAgentFactory agentFactory = new(secrets.AzureOpenAiEndpoint, secrets.AzureOpenAiKey);

AgentSkillsFactory agentSkillsFactory = new AgentSkillsFactory();
AgentSkillsDotNet.AgentSkills agentSkills = agentSkillsFactory.GetAgentSkills("TestData\\AgentSkills");

string skillsInstructions = agentSkills.GetInstructions();

IList<AITool> tools = agentSkills.GetAsTools();


tools.Add(AIFunctionFactory.Create(PythonRunner.RunPhytonScript, name: "execute_python"));

AzureOpenAIAgent agent = agentFactory.CreateAgent(new AgentOptions
{
    Model = OpenAIChatModels.Gpt41Mini,
    Instructions = $"""
                    You are an nice a AI with various skills
                    ## Skills available:
                    {skillsInstructions}

                    Only call '{AgentSkillsAsToolsOptions.DefaultReadSkillFileContentToolName}' tool 
                    once you have used '{AgentSkillsAsToolsOptions.DefaultGetSpecificSkillToolName}' tool
                    """,
    Tools = tools,
    RawToolCallDetails = details =>
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine(details.ToString());
        Console.ResetColor();
    }
});

AgentSession session = await agent.CreateSessionAsync();

Console.OutputEncoding = Encoding.UTF8;
while (true)
{
    Console.Write("> ");
    string message = Console.ReadLine() ?? "";
    AgentResponse response = await agent.RunAsync(message, session);
    Console.WriteLine(response);
    Console.WriteLine();
    Console.ForegroundColor = ConsoleColor.Gray;
    Console.WriteLine($"Input Tokens: {response.Usage!.InputTokenCount} - " +
                      $"Output Tokens: {response.Usage.OutputTokenCount} ");
    Console.ResetColor();
    Utils.Separator();
}