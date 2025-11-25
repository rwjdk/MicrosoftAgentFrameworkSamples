// See https://aka.ms/new-console-template for more information

using Amazon;
using Amazon.BedrockRuntime;
using Microsoft.Agents.AI;

string apiKey = "<YourApiKey>";
Environment.SetEnvironmentVariable("AWS_BEARER_TOKEN_BEDROCK", apiKey);

AmazonBedrockRuntimeClient runtimeClient = new(RegionEndpoint.EUNorth1);

ChatClientAgent agent = new(runtimeClient.AsIChatClient("eu.anthropic.claude-haiku-4-5-20251001-v1:0"));

AgentRunResponse response = await agent.RunAsync("Hello");
Console.WriteLine(response);