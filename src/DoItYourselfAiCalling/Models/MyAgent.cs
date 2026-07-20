using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace DoItYourselfAiCalling.Models;

internal class MyAgent
{
    public required Provider Provider { get; set; }
    public string? Instructions { get; set; }

    public async Task<MyResponse> RunAsync(string message)
    {
        return await RunAsync(new List<MyMessage>
        {
            new()
            {
                Role = "user",
                Content = message
            }
        });
    }

    public async Task<MyResponse> RunAsync(IList<MyMessage> messages)
    {
        IList<MyMessage> messagesToSend = messages;
        if (!string.IsNullOrWhiteSpace(Instructions))
        {
            messagesToSend.Insert(0, new MyMessage
                {
                    Role = "system",
                    Content = Instructions
                }
            );
        }

        //Message part of payload
        JsonArray jsonMessages = [];
        foreach (MyMessage message in messagesToSend)
        {
            jsonMessages.Add(new JsonObject
            {
                {"role", message.Role},
                {"content", message.Content}
            });
        }

        JsonObject json = new()
        {
            {"messages", jsonMessages}
        };

        HttpClient httpClient = new();
        httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {Provider.ApiKey}");
        
        string urlToUse = Provider.Endpoint;
        string suffix = "/openai/v1";
        if (urlToUse.EndsWith(suffix))
        {
            //strip off the default mentioned in Azure Portal if given
            urlToUse = urlToUse.Remove(urlToUse.Length - suffix.Length);
        }
        urlToUse += $"/openai/deployments/{Provider.Model}/chat/completions?api-version=2025-04-01-preview";
        
        HttpResponseMessage httpResponse = await httpClient.PostAsJsonAsync(new Uri(urlToUse), json);
        string jsonResponse = await httpResponse.Content.ReadAsStringAsync();

        InternalResponse internalResponse = JsonSerializer.Deserialize<InternalResponse>(jsonResponse)!;

        return new MyResponse
        {
            Messages = [internalResponse.Choices[0].Message]
        };
    }

    private class InternalResponse
    {
        [JsonPropertyName("id")]
        public required string Id { get; set; }

        [JsonPropertyName("choices")]
        public required InternalResponseChoice[] Choices { get; set; }
    }

    private class InternalResponseChoice
    {
        [JsonPropertyName("finish_reason")]
        public required string FinishReason { get; set; }

        [JsonPropertyName("message")]
        public required MyMessage Message { get; set; }
    }
}