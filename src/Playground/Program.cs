//WARNING: This is a playground area for the creator of the Repo to test and tinker. Nothing in this project is as such educational and might not even execute properly

//Notes
//- Microsoft.Agents.AI.Hosting.AgentCatalog TODO: Guess this is something to be used in AI Foundry

#pragma warning disable OPENAI001
using Azure.AI.OpenAI;
using OpenAI.Batch;
using OpenAI.Files;
using Shared;
using System.ClientModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI;
using OpenAI.Chat;

Console.Clear();

Secrets secrets = SecretManager.GetSecrets();

OpenAIClient client = new(secrets.OpenAiApiKey);
ChatClientAgent agent = client
    .GetChatClient("gpt-5-mini")
    .AsAIAgent(
        options: new ChatClientAgentOptions
        {
            ChatOptions = new ChatOptions
            {
                RawRepresentationFactory = _ => new ChatCompletionOptions
                {
                    ReasoningEffortLevel = ChatReasoningEffortLevel.Minimal,
                    ServiceTier = new ChatServiceTier("priority") // auto, flex, default, priority
                }
            }
        });

AgentResponse response = await agent.RunAsync("What is the capital of France?");