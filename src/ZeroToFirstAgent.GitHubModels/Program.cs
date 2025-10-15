/* Steps:
 * 1: Get a GitHubPAT Token with GitHub Models access (https://github.com/marketplace?type=models > Choose a Model and follow instructions)
 * 2: Add Nuget Packages (Azure.AI.Inference + Microsoft.Extensions.AI.AzureAIInference + Microsoft.Agents.AI)
 * 3: Create an ChatCompletionsClient
 * 4: Get an .AsIChatClient for the model and Create an AI Agent from it
 * 5: Call RunAsync or RunStreamingAsync
 */

using Azure;
using Azure.AI.Inference;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

const string gitHubPatToken = "<YourGitHubPATTokenWithModelAccess>";
const string model = "<yourModelName>"; //Example: deepseek/DeepSeek-V3-0324

ChatClientAgent agent = new ChatCompletionsClient(
    new Uri("https://models.github.ai/inference"),
    new AzureKeyCredential(gitHubPatToken),
    new AzureAIInferenceClientOptions()).AsIChatClient(model).CreateAIAgent();

AgentRunResponse response = await agent.RunAsync("What is the Capital of Denmark?");
Console.WriteLine(response);

Console.WriteLine("---");

await foreach (AgentRunResponseUpdate update in agent.RunStreamingAsync("How to make soup?"))
{
    Console.Write(update);
}