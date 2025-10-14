//WARNING: This is a playground area for the creator of the Repo to test and tinker. Nothing in this project is as such educational and might not even execute properly

using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Chat;
using Shared;
using System.ClientModel;
using System.Text;
using Shared.Extensions;

//Notes
//- Microsoft.Agents.AI.Hosting.AgentCatalog TODO: Guess this is something to be used in AI Foundry

Configuration configuration = ConfigurationManager.GetConfiguration();

//Define the basics
AzureOpenAIClient azureOpenAiClient = new(new Uri(configuration.AzureOpenAiEndpoint), new ApiKeyCredential(configuration.AzureOpenAiKey));
ChatClient chatClient = azureOpenAiClient.GetChatClient("gpt-5-nano");
IList<AITool> aiTools = [AIFunctionFactory.Create(WhatIsTheUsersName)];
string? myInstructions = "Use tools to answer all questions.";
string myQuestion = "What is my Name?";

ChatClientAgent agent = chatClient.CreateAIAgent(
    options: new ChatClientAgentOptions
    {
        Instructions = "You are a nice AI",
        ChatOptions = new ChatOptions
        {
            RawRepresentationFactory = _ => new ChatCompletionOptions
            {
#pragma warning disable OPENAI001
                ReasoningEffortLevel = "minimal",
#pragma warning restore OPENAI001
            },
            Tools = [] //Tools goes here
        }
    });

AgentRunResponse response1 = await agent.RunAsync(myQuestion);
Console.WriteLine(response1);
response1.Usage.OutputAsInformation();

Console.WriteLine("-------------------------------------");

//This setup work (because the tools are given via the chat-options)
Console.WriteLine("WORKING EXAMPLE:");
ChatClientAgent agentSetupThatWork = chatClient.CreateAIAgent(
    options: new ChatClientAgentOptions(instructions: myInstructions)
    {
        ChatOptions = new ChatOptions
        {
            RawRepresentationFactory = (Func<IChatClient, object?>)(_ => new ChatCompletionOptions()
            {
#pragma warning disable OPENAI001
                ReasoningEffortLevel = "minimal",
#pragma warning restore OPENAI001
            }),
            Tools = aiTools
        },
    });

AgentRunResponse response2 = await agentSetupThatWork.RunAsync(myQuestion);
Console.WriteLine(response2);
response1.Usage.OutputAsInformation();

//Tool
static string WhatIsTheUsersName()
{
    return "John AI";
}