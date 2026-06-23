using System.ComponentModel;
using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using OpenAI.Chat;
using Shared;

namespace Playground.Tools;

public class AgentBuilderTool
{
    public async Task<string> RunSubAgent([Description("Give the agent a funny name")]string agentName, string systemInstructions, string prompt)
    {
        Utils.Green($"Sub-agent '{agentName}': Instructions: '{systemInstructions}' - Prompt: '{prompt}'");

        AzureOpenAIClient client = ClientHelper.GetAzureOpenAIClient();
        ChatClientAgent agent = client.GetChatClient("gpt-4.1-mini").AsAIAgent(instructions: systemInstructions);
        return (await agent.RunAsync(prompt)).ToString();
    }
}