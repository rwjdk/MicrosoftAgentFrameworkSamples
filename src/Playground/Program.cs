//WARNING: This is a playground area for the creator of the Repo to test and tinker. Nothing in this project is as such educational and might not even execute properly
#pragma warning disable OPENAI002
using Microsoft.Extensions.AI;

using Shared;
using Azure.AI.OpenAI;
using HyperlightSandbox.Api;
using HyperlightSandbox.Guest.Python;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Hyperlight;
using OpenAI.Chat;

Utils.Init("Playground");

AzureOpenAIClient client = ClientHelper.GetAzureOpenAIClient();
ChatClient chatClient = client.GetChatClient("gpt-5.4-mini");

//await Javascript();
await Python();
//await JavascriptWithTools();

async Task Javascript()
{
    Utils.Yellow("JavaScript Demo");
    HyperlightCodeActProviderOptions options = HyperlightCodeActProviderOptions.CreateForJavaScript();
    ChatClientAgent agent = chatClient.AsAIAgent(new ChatClientAgentOptions
    {
        ChatOptions = new ChatOptions
        {
            Instructions = "Always use use 'execute_code' tool for any math calculations (write the code in Javascript). Show result + Code you used"
        },
        AIContextProviders = [new HyperlightCodeActProvider(options)]
    });

    AgentResponse response = await agent.RunAsync("What is the 20th Fibonacci number divided by 42 and then subtract 30?");
    Console.WriteLine(response);
}

async Task Python()
{
    Utils.Yellow("Python Demo");

    HyperlightCodeActProviderOptions options = HyperlightCodeActProviderOptions.CreateForWasm(modulePath: "???");
    ChatClientAgent agent = chatClient.AsAIAgent(new ChatClientAgentOptions
    {
        ChatOptions = new ChatOptions
        {
            Instructions = "Always use use 'execute_code' tool for any math calculations (write the code in Python). Show result + Code you used"
        },
        AIContextProviders = [new HyperlightCodeActProvider(options)]
    });

    AgentResponse response = await agent.RunAsync("What is the 20th Fibonacci number divided by 42 and then subtract 30?");
    Console.WriteLine(response);
}

async Task JavascriptWithTools()
{
    Utils.Yellow("Javascript with Tools Demo");
    HyperlightCodeActProviderOptions options = HyperlightCodeActProviderOptions.CreateForJavaScript();
    options.Tools = [AIFunctionFactory.Create(WeatherTool.GetWeather, "get_weather_for_city")];
    
    ChatClientAgent agent = chatClient.AsAIAgent(new ChatClientAgentOptions
    {
        ChatOptions = new ChatOptions
        {
            Instructions = "Always use use 'execute_code' tool for any requests (write the code in Javascript). Show result + Code you used"
        },
        AIContextProviders = [new HyperlightCodeActProvider(options)]
    });

    AgentResponse response = await agent.RunAsync("What is the weather like in Paris?");
    Console.WriteLine(response);
}

public static class WeatherTool
{
    public static string GetWeather(string city)
    {
        return "It is Sunny and 19 Degrees";
    }
}

/*


HyperlightCodeActProvider hyperlightCodeActProvider = new HyperlightCodeActProvider(options);

using HyperlightExecuteCodeFunction executeCode = new HyperlightExecuteCodeFunction(options);
string instructions = executeCode.BuildInstructions();*/





