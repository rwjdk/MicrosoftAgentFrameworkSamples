using AdvancedRAGTechniques.EmbeddingOptions;
using AdvancedRAGTechniques.SearchOptions;
using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel.Connectors.SqlServer;
using OpenAI;
using Shared;
using Shared.Extensions;
using UsingRAGInAgentFramework.Models;

namespace AdvancedRAGTechniques;

public static class Option2EnhanceEmbeddings
{
    public static async Task Run(bool importData, Movie[] movieDataForRag, ChatMessage question, AzureOpenAIClient client, SqlServerCollection<Guid, MovieVectorStoreRecord> collection, Configuration configuration)
    {
        if (importData)
        {
            await EnhanceDataEmbedding.Embed(client, configuration, collection, movieDataForRag);
        }

        EnhancedSearchTool searchTool = new(collection);
        AIAgent agent = client.GetChatClient(configuration.ChatDeploymentName)
            .CreateAIAgent(
                instructions: """
                              You are an expert a set of made up movies given to you (aka don't consider movies from your world-knowledge)
                              When using tools use keywords only based on the users question so it is better for similarity search
                              When listing the movies (list their titles, plots and ratings)
                              """,
                tools: [AIFunctionFactory.Create(searchTool.SearchVectorStore)])
            .AsBuilder()
            .Use(Middleware.FunctionCallMiddleware)
            .Build();

        AgentRunResponse response = await agent.RunAsync(question);
        Console.WriteLine(response);
        response.Usage.OutputAsInformation();
    }
}