//YouTube video that cover this sample: https://youtu.be/6i1Rs0MkBDQ
// ReSharper disable UnusedVariable

#pragma warning disable OPENAI001

using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenAI;
using OpenAI.Chat;
using OpenTelemetry;
using Shared;
using System.ClientModel;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Trace;


Configuration configuration = ConfigurationManager.GetConfiguration();

AzureOpenAIClient client = new(new Uri(configuration.AzureOpenAiEndpoint), new ApiKeyCredential(configuration.AzureOpenAiKey));

ChatClientAgent noSettingAgent = client.GetChatClient(configuration.ChatDeploymentName).CreateAIAgent();

ChatClientAgent agent = client.GetChatClient(configuration.ChatDeploymentName).CreateAIAgent(
    instructions: "You are a cool surfer dude",
    tools: [ /*Todo: In a separate video*/]);

#region Lets set all the options-parameters!

//Service provider (explained later)
HostApplicationBuilder builder = Host.CreateApplicationBuilder();
builder.Services.AddSingleton(new MySpecialService());
ServiceProvider serviceProvider = builder.Services.BuildServiceProvider();

// OpenTelemetry
string sourceName = Guid.NewGuid().ToString("N");
using var tracerProvider = Sdk.CreateTracerProviderBuilder()
    .AddSource(sourceName)
    .AddConsoleExporter()
    .Build();

AIAgent agentWithAllSettings = client.GetChatClient(configuration.ChatDeploymentName).CreateAIAgent
    (
        //Optional system instructions that define the agent's behavior and personality.
        instructions: "Speak like a Pirate",

        //***************************************************************************************************************************

        //Optional name for the agent for identification purposes.
        name: "My Agent",

        //***************************************************************************************************************************

        //Optional description of the agent's capabilities and purpose.
        description: "Description that is not used by the AI, but some of the online Agent Framework have a description",

        //***************************************************************************************************************************

        //Optional collection of AI tools that the agent can use during conversations.
        tools: [], //Will be covered in a separate video

        //***************************************************************************************************************************

        //Provides a way to customize the creation of the underlying IChatClient used by the agent.
        clientFactory: chatClient =>
        {
            return new ConfigureOptionsChatClient(chatClient, options =>
            {
                options.RawRepresentationFactory = _ => new ChatCompletionOptions
                {
                    ReasoningEffortLevel = ChatReasoningEffortLevel.High,
                };
            });
        },

        //***************************************************************************************************************************

        //Optional logger factory for enabling logging within the agent.
        loggerFactory: LoggerFactory.Create(loggingBuilder => { loggingBuilder.AddConsole(); }),

        //***************************************************************************************************************************

        //An optional IServiceProvider to use for resolving services required by the AIFunction instances being invoked.
        services: serviceProvider
    )
    .AsBuilder()
    .UseOpenTelemetry(sourceName) //Middleware
    .Build();

AgentRunResponse response = await agentWithAllSettings.RunAsync("What is the capital of France?");
Console.WriteLine(response);

#endregion

#region Even more options via ChatClientAgentOptions

ChatClientAgent advancedAgent = client.GetChatClient(configuration.ChatDeploymentName).CreateAIAgent(
    new ChatClientAgentOptions
    {
        ChatOptions = new ChatOptions
        {
            //Various options on model behaviour (Temperature, Reasoning Effort, ect.)
        },
        AIContextProviderFactory = null, //Option to intercept before and after LLM call
        ChatMessageStoreFactory = null, //Option to inject a store for thread conversations,
        Instructions = "Speak like a Pirate",
        Name = "My Agent",
        Description = "Description that is not used by the AI, but some of the online Agent Framework have a description",
        Id = "1234",
        UseProvidedChatClientAsIs = false
    },
    clientFactory: chatClient =>
    {
        return new ConfigureOptionsChatClient(chatClient, options =>
        {
            options.RawRepresentationFactory = _ => new ChatCompletionOptions
            {
                ReasoningEffortLevel = ChatReasoningEffortLevel.Low,
            };
        });
    },
    loggerFactory: LoggerFactory.Create(loggingBuilder => { loggingBuilder.AddConsole(); }),
    services: serviceProvider
);

#endregion

public class MySpecialService
{
    //Some code should go here
}