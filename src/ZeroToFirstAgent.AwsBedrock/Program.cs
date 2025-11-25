// See https://aka.ms/new-console-template for more information

using Amazon;
using Amazon.BedrockRuntime;
using Microsoft.Agents.AI;

Environment.SetEnvironmentVariable("AWS_BEARER_TOKEN_BEDROCK", "<YourApiKey>");

AmazonBedrockRuntimeClient runtimeClient = new(RegionEndpoint.EUNorth1); //<-- Your region

ChatClientAgent agent = new(runtimeClient.AsIChatClient("<YourModel>"));

AgentRunResponse response = await agent.RunAsync("Hello");
Console.WriteLine(response);