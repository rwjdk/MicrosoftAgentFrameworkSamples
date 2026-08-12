using System.ClientModel;
using AgentFrameworkToolkit.AzureOpenAI;
using AgentFrameworkToolkit.OpenAI;
using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI.Chat;
using OpenAI.Responses;
using Shared;
#pragma warning disable OPENAI001 //https://github.com/openai/openai-dotnet/issues/1130

Utils.Init("It is Time to Switch to OpenAI Responses API");

Secrets secrets = SecretsManager.GetSecrets();

AzureOpenAIClient client = new(new Uri(secrets.AzureOpenAiEndpoint), new ApiKeyCredential(secrets.AzureOpenAiKey));

AzureOpenAIAgentFactory azureOpenAIAgentFactory = new(new AzureOpenAIConnection
{
    Endpoint = secrets.AzureOpenAiEndpoint,
    ApiKey = secrets.AzureOpenAiKey
});

string question = "What is the Weather like in Paris?";
AIFunction tool = AIFunctionFactory.Create(GetWeather, "get_weather_for_city");

//string modelToUse = "gpt-5.4-mini";
const string modelToUse = "gpt-5.6-luna";

await ChatClientWithTool(modelToUse);

await ChatClientWithLowReasoning(modelToUse);

await ChatClientWithToolAndLowReasoning(modelToUse);

await ResponsesApiWithToolAndLowReasoning(modelToUse);

await AgentFrameworkToolkitWithToolAndLowReasoning(modelToUse);

return;

static string GetWeather(string city)
{
    return "It is sunny and 19 degrees";
}

async Task ChatClientWithTool(string model)
{
    try
    {
        Utils.Yellow($"ChatClient with Tool answering Question: '{question}' with model: {model}");
        AIAgent agent = client.GetChatClient(model).AsAIAgent(tools: [tool]);
        AgentResponse response = await agent.RunAsync(question);
        Console.WriteLine(response);
    }
    catch (Exception e)
    {
        Utils.Red($"Error: {e.Message}");
    }
    finally
    {
        Utils.Gray("---");
    }
}

async Task ChatClientWithLowReasoning(string model)
{
    try
    {
        Utils.Yellow($"ChatClient with Reasoning answering Question: '{question}' with model: {model}");
        AIAgent agent = client.GetChatClient(model).AsAIAgent(new ChatClientAgentOptions
        {
            ChatOptions = new ChatOptions
            {
                RawRepresentationFactory = _ => new ChatCompletionOptions
                {
                    ReasoningEffortLevel = ChatReasoningEffortLevel.Low
                }
            }
        });
        AgentResponse response = await agent.RunAsync(question);
        Console.WriteLine(response);
    }
    catch (Exception e)
    {
        Utils.Red($"Error: {e.Message}");
    }
    finally
    {
        Utils.Gray("---");
    }
}

async Task ChatClientWithToolAndLowReasoning(string model)
{
    try
    {
        Utils.Yellow($"ChatClient with Tool and Reasoning answering Question: '{question}' with model: {model}");
        AIAgent agent = client.GetChatClient(model).AsAIAgent(new ChatClientAgentOptions
        {
            ChatOptions = new ChatOptions
            {
                Tools = [tool],
                RawRepresentationFactory = _ => new ChatCompletionOptions
                {
                    ReasoningEffortLevel = ChatReasoningEffortLevel.Low
                }
            }
        });
        AgentResponse response = await agent.RunAsync(question);
        Console.WriteLine(response);
    }
    catch (Exception e)
    {
        Utils.Red($"Error: {e.Message}");
    }
    finally
    {
        Utils.Gray("---");
    }
}

async Task ResponsesApiWithToolAndLowReasoning(string model)
{
    try
    {
        Utils.Yellow($"ResponsesAPI with Tool and Reasoning answering Question: '{question}' with model: {model}");
        AIAgent agent = client.GetResponsesClient().AsAIAgent(new ChatClientAgentOptions
        {
            ChatOptions = new ChatOptions
            {
                Tools = [tool],
                RawRepresentationFactory = _ => new CreateResponseOptions
                {
                    ReasoningOptions = new ResponseReasoningOptions
                    {
                        ReasoningEffortLevel = ResponseReasoningEffortLevel.Low,
                    }
                }
            }
        }, model);

        AgentResponse response = await agent.RunAsync(question);
        Console.WriteLine(response);
    }
    catch (Exception e)
    {
        Utils.Red($"Error: {e.Message}");
    }
    finally
    {
        Utils.Gray("---");
    }
}

async Task AgentFrameworkToolkitWithToolAndLowReasoning(string model)
{
    try
    {
        Utils.Yellow($"AgentFrameworkToolkit with Tool and Reasoning answering Question: '{question}' with model: {model}");
        AzureOpenAIAgent agent = azureOpenAIAgentFactory.CreateAgent(new AgentOptions
        {
            ClientType = ClientType.ResponsesApi,
            Model = model,
            Tools = [tool],
            ReasoningEffort = OpenAIReasoningEffort.Low
        });
        AgentResponse response = await agent.RunAsync(question);
        Console.WriteLine(response);
    }
    catch (Exception e)
    {
        Utils.Red($"Error: {e.Message}");
    }
    finally
    {
        Utils.Gray("---");
    }
}