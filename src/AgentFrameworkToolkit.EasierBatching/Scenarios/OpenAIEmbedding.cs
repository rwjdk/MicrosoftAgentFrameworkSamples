using AgentFrameworkToolkit.OpenAI.Batching;
using Shared;
#pragma warning disable AFT999

namespace AgentFrameworkToolkit.EasierBatching.Scenarios;

public static class OpenAIEmbedding
{
    public static async Task RunSampleAsync(Secrets secrets)
    {
        OpenAIBatchRunner batchRunner = new(secrets.OpenAiApiKey);

        EmbeddingBatchRun run = await batchRunner.RunEmbeddingBatchAsync(new EmbeddingBatchOptions
            {
                WaitUntilCompleted = true,
                Model = "text-embedding-3-small",
                //GenerationOptions = todo
            },
            [
                EmbeddingBatchRequest.Create("First Text to Embed"),
                EmbeddingBatchRequest.Create("Second Text to Embed")
            ]
        );

        IList<EmbeddingBatchRunResult> results = await run.GetResultAsync();

        foreach (EmbeddingBatchRunResult result in results)
        {
            if (result.Error == null)
            {
                //Success
                Utils.Gray(result.Request+ " => ");
                Utils.Green(result.Response!.Dimensions+" dimensions");
            }
            else
            {
                //Error
                Utils.Red(result.Error.ErrorMessage!);
            }
            Utils.Separator();
        }
    }

}