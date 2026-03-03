using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using OpenAI.Chat;
using Shared;
using StructuredOutput.Models;
using System.ClientModel;

Utils.Init("Structured Output");
(Uri endpoint, ApiKeyCredential apiKey) = SecretsManager.GetAzureOpenAICredentials();
AzureOpenAIClient client = new(endpoint, apiKey);

const string question = "What are the top 10 Movies according to IMDB?";
const string instructions = "You are an expert in IMDB Lists";
const string model = "gpt-4.1";

AIAgent agent = client.GetChatClient(model).AsAIAgent(instructions);

Utils.Red("Without Structured Output 👎");
AgentResponse unstructuredResponse = await agent.RunAsync(question);
Console.WriteLine(unstructuredResponse);

Utils.Separator();

Utils.Green("With Structured Output 👍");
AgentResponse<List<Movie>> structuredResponse = await agent.RunAsync<List<Movie>>(question);

List<Movie> movies = structuredResponse.Result;

int counter = 1;
foreach (Movie movie in movies)
{
    Console.WriteLine($"{counter}: " +
                      $"{movie.Title} ({movie.YearOfRelease}) - " +
                      $"Genre: {movie.Genre} - " +
                      $"Director: {movie.Director} - " +
                      $"IMDB Score: {movie.ImdbScore}");
    counter++;
}