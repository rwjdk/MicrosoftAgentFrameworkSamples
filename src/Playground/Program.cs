//WARNING: This is a playground area for the creator of the Repo to test and tinker. Nothing in this project is as such educational and might not even execute properly

//Notes
//- Microsoft.Agents.AI.Hosting.AgentCatalog TODO: Guess this is something to be used in AI Foundry

#pragma warning disable OPENAI001
using Azure.AI.OpenAI;
using OpenAI.Batch;
using OpenAI.Files;
using Shared;
using System.ClientModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using OpenAI;

Console.Clear();

Secrets secrets = SecretManager.GetSecrets();

OpenAIClient client = new(secrets.OpenAiApiKey);
OpenAIFileClient fileClient = client.GetOpenAIFileClient();
BatchClient batchClient = client.GetBatchClient();

OpenAIFile file = await fileClient.UploadFileAsync("requestData.jsonl", new FileUploadPurpose("batch"));

BinaryContent createBatchPayload = BinaryContent.CreateJson(new CreateBatchRequest
{
    InputFileId = file.Id,
    Endpoint = "/v1/chat/completions",
    CompletionWindow = "24h",
    Metadata = new Dictionary<string, string>
    {
        ["source"] = "Playground"
    }
});

CreateBatchOperation createBatchOperation =
    await batchClient.CreateBatchAsync(createBatchPayload, waitUntilCompleted: false);

Console.WriteLine($"Rehydration token: {createBatchOperation.RehydrationToken}");

await createBatchOperation.UpdateStatusAsync();
ClientResult batchResult = await createBatchOperation.GetBatchAsync(options: null);

string batchJson = batchResult.GetRawResponse().Content.ToString();
using JsonDocument json = JsonDocument.Parse(batchJson);
string? batchId = json.RootElement.TryGetProperty("id", out JsonElement idElement)
    ? idElement.GetString()
    : null;
string? status = json.RootElement.TryGetProperty("status", out JsonElement statusElement)
    ? statusElement.GetString()
    : null;

Console.WriteLine($"Batch id: {batchId ?? "unknown"}");
Console.WriteLine($"Current batch status: {status ?? "unknown"}");

internal sealed class CreateBatchRequest
{
    [JsonPropertyName("input_file_id")]
    public required string InputFileId { get; init; }

    [JsonPropertyName("endpoint")]
    public required string Endpoint { get; init; }

    [JsonPropertyName("completion_window")]
    public required string CompletionWindow { get; init; }

    [JsonPropertyName("metadata")]
    public Dictionary<string, string>? Metadata { get; init; }
}
