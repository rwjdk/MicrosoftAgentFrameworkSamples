//WARNING: This is a playground area for the creator of the Repo to test and tinker. Nothing in this project is as such educational and might not even execute properly
#pragma warning disable OPENAI001
using Shared;
using Microsoft.Agents.AI;
using OpenAI;
using OpenAI.Chat;

Utils.Init("Playground");

Secrets secrets = SecretsManager.GetSecrets();

OpenAIClient client = new(secrets.OpenAiApiKey);
AIAgent agent = client
    .GetChatClient("gpt-5-mini")
    .AsAIAgent("You are a nice AI");

AgentResponse response = await agent.RunAsync("What is the capital of France?");

Console.WriteLine(response);