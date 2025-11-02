using Azure.AI.Agents.Persistent;
using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI;
using Shared;
using System.ClientModel;
using System.Text;
using Microsoft.Agents.AI.Hosting;
using ConfigurationManager = Shared.ConfigurationManager;

#pragma warning disable OPENAI001
Configuration configuration = ConfigurationManager.GetConfiguration();
string model = "gpt-4.1";

//The Raw client (OpenAIClient and AzureOpenAIClient works that same)
OpenAIClient rawOpenAiClient = new(configuration.OpenAiApiKey);
AzureOpenAIClient rawAzureOpenAiClient = new(new Uri(configuration.AzureOpenAiEndpoint), new ApiKeyCredential(configuration.AzureOpenAiKey));

//--------------------------------------------------------------------------

//Azure AI Foundry's PersistentAgentsClient is unique to that service
PersistentAgentsClient persistentAgentsClient = new(configuration.AzureAiFoundryAgentEndpoint, new AzureCliCredential());

//--------------------------------------------------------------------------

//IChatClient is Microsoft.Extension.AI's "version of AIAgent" [This is what other non-OpenAI Providers implement to be compatible]
IChatClient iChatClient = rawOpenAiClient.GetChatClient(model).AsIChatClient();

//--------------------------------------------------------------------------

//All Agents are AI Agents (which is an abstract class)
AIAgent aiAgentFromChatClient = rawOpenAiClient.GetChatClient(model).CreateAIAgent();
AIAgent aiAgentFromResponsesClient = rawOpenAiClient.GetOpenAIResponseClient(model).CreateAIAgent();
AIAgent aiAgentFromPersistentClient = await persistentAgentsClient.GetAIAgentAsync(persistentAgentsClient.Administration.CreateAgent(model).Value.Id);
AIAgent aiAgentFromIChatClient = new ChatClientAgent(iChatClient);

//--------------------------------------------------------------------------

//Most is however a ChatClientAgent behind the scenes
ChatClientAgent chatClientAgentFromChatClient = rawOpenAiClient.GetChatClient(model).CreateAIAgent();
ChatClientAgent chatClientAgentFromResponsesClient = rawOpenAiClient.GetOpenAIResponseClient(model).CreateAIAgent();
ChatClientAgent chatClientAgentFromPersistentClient = await persistentAgentsClient.GetAIAgentAsync(persistentAgentsClient.Administration.CreateAgent(model).Value.Id);
ChatClientAgent chatClientAgentFromIChatClient = new ChatClientAgent(iChatClient);

//This is good, since that is the only AgentType that support ".RunAsync<>" (Structured output)

//--------------------------------------------------------------------------

//Middleware Builder can only produce an AI Agent :-(
AIAgent aiAgentWithOpenTelemetryMiddleware = rawOpenAiClient
    .GetChatClient(model)
    .CreateAIAgent()
    .AsBuilder()
    .UseOpenTelemetry("test")
    .Build(); //<< Always AIAgent

AIAgent aiAgentWithFunctionCallingMiddleware = rawOpenAiClient
    .GetChatClient(model)
    .CreateAIAgent()
    .AsBuilder()
    .Use(FunctionCallMiddleware)
    .Build(); //<< Always AIAgent

AIAgent aiAgentWithOpenTelemetryAndFunctionCallingMiddleware = rawOpenAiClient
    .GetChatClient(model)
    .CreateAIAgent()
    .AsBuilder()
    .Use(FunctionCallMiddleware)
    .UseOpenTelemetry("test")
    .Build(); //<< Always AIAgent

//These AsBuilder() agents are often DelegatingAIAgents (meaning they are not based directly on IChatClient)
if (aiAgentWithOpenTelemetryMiddleware is DelegatingAIAgent delegatingAIAgent1)
{
    Console.WriteLine("This is a DelegatingAIAgent");
}

//Same goes for the FunctionCallingMiddleware Agent
if (aiAgentWithFunctionCallingMiddleware is DelegatingAIAgent delegatingAIAgent2)
{
    Console.WriteLine("This is also a DelegatingAIAgent");
}

//NOTE: They are also a 'AnonymousDelegatingAIAgent' (which is internal and there to support AsBuilder...)

//In an OpenTelemetry scenario this delegating Agent is also an OpenTelemetryAgent (which implement DelegatingAIAgent)
if (aiAgentWithOpenTelemetryMiddleware is OpenTelemetryAgent openTelemetryAgent)
{
    Console.WriteLine("This is a OpenTelemetryAgent");
}

//In an FunctionCallingMiddleware scenario this delegating Agent is also technically a FunctionInvocationDelegatingAgent
//but since that class is internal, you can't 'work with it' as such
string fullNameOfAiAgentWithFunctionCallingMiddleware = aiAgentWithFunctionCallingMiddleware.GetType().FullName!;

//NOTE: The 'aiAgentWithOpenTelemetryAndFunctionCallingMiddleware' become the type of agent the first middleware is

//--------------------------------------------------------------------------

//You can also build a OpenAI specific DelegatingAIAgents based on a raw chat-client (can't see a reason for doing this really)
OpenAIChatClientAgent openAIChatClientAgent = new OpenAIChatClientAgent(rawOpenAiClient.GetChatClient(model));
OpenAIResponseClientAgent openAIResponsesClientAgent = new OpenAIResponseClientAgent(rawOpenAiClient.GetOpenAIResponseClient(model));

//A Copilot Studio Agent
//CopilotStudioAgent copilotStudioAgent = new CopilotStudioAgent(new CopilotClient(<settings>));

//And a HostAgent (not fully understood this one, but is used for A2A)
AIHostAgent aiHostAgent = new AIHostAgent(chatClientAgentFromChatClient, new InMemoryAgentThreadStore());

Console.WriteLine();
//--------------------------------------------------------------------------

/* Other Internal AIAgents
- WorkflowHostAgent (when you turn a Workflow into an Agent)
- A2AAgent (when you turn a A2AClient into an Agent)
*/

Console.WriteLine();
//--------------------------------------------------------------------------

//So what does all of this mean?... 3 things IMHO:

// 1. If you can, get a ChatClientAgent (due to its Structure Output capabilities)
// 2. If not possible, then get an AIAgent (and live with the more cumbersome Structured Output)
// 3. Forget the rest exist (if you know one of the others, you properly do not need this video)

//--------------------------------------------------------------------------

Console.ReadKey();

async ValueTask<object?> FunctionCallMiddleware(AIAgent callingAgent, FunctionInvocationContext context, Func<FunctionInvocationContext, CancellationToken, ValueTask<object?>> next, CancellationToken cancellationToken)
{
    StringBuilder functionCallDetails = new();
    functionCallDetails.Append($"- Tool Call: '{context.Function.Name}'");
    if (context.Arguments.Count > 0)
    {
        functionCallDetails.Append($" (Args: {string.Join(",", context.Arguments.Select(x => $"[{x.Key} = {x.Value}]"))}");
    }

    Utils.WriteLineDarkGray(functionCallDetails.ToString());

    return await next(context, cancellationToken);
}