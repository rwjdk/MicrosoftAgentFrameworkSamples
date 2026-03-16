using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Shared;
using System.ClientModel;
using OpenAI.Chat;

Console.Clear();

Secrets secrets = SecretsManager.GetSecrets();

AzureOpenAIClient client = new(
    new Uri(secrets.AzureOpenAiEndpoint),
    new ApiKeyCredential(secrets.AzureOpenAiKey));

string text = "2+2 = 4, 10*10 = 100";
text = File.ReadAllText("Book.txt");
AIAgent agent = client.GetChatClient("gpt-5-mini").AsAIAgent(
    instructions: "Your only knowledge is the text: " + text
    );

AgentSession session = await agent.CreateSessionAsync();

while (true)
{
    Console.Write("> ");
    string input = Console.ReadLine() ?? "";
    AgentResponse response = await agent.RunAsync(input, session);
    Console.WriteLine(response);

    Utils.Yellow("Token Usages (All gpt-5-mini prices are per 1 Million tokens as of 16th of March 2026)");
    Utils.Gray("- Input       ($0.25)            : " + response.Usage!.InputTokenCount);
    Utils.Gray("  - Cached    ($0.025)           : " + response.Usage!.CachedInputTokenCount);
    Utils.Gray("- Output      ($2.00)            : " + response.Usage!.OutputTokenCount);
    Utils.Gray("  - Reasoning (billed as output) : " + response.Usage!.ReasoningTokenCount);
    Utils.Separator();
}