using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OllamaSharp;

IChatClient client = new OllamaApiClient("http://localhost:11434", "llama3.2:1b");
AIAgent agent = new ChatClientAgent(client);
AgentRunResponse response = await agent.RunAsync("What is the Capital of Sweden?");
Console.WriteLine(response);