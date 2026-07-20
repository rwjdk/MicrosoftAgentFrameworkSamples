using DoItYourselfAiCalling.Helpers;
using DoItYourselfAiCalling.Models;

Console.Clear();
Console.WriteLine("Sample: Azure OpenAI with HTTP Client");

string endpoint = EnvironmentVariableHelper.GetValueOrAsk("myEndpoint");
string apiKey = EnvironmentVariableHelper.GetValueOrAsk("myApiKey", secret: true);

MyAgent agent = new MyAgent
{
    Provider = new Provider(endpoint, apiKey, "gpt-5.6-luna"),
    Instructions = "You are a nice AI!"
};

//Part 1: Normal Call
string question = "What is the capital of France?";
Console.WriteLine($"Q1: {question}");
MyResponse response = await agent.RunAsync(question);
Console.WriteLine($"A1: {response}");