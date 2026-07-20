using System.Net.Http.Json;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace DoItYourselfAiCalling.Models;

internal class MyAgent
{
    public required Provider Provider { get; set; }
    public string? Instructions { get; set; }
    public JsonObject? ResponseFormat { get; set; }
    public IList<MyTool> Tools { get; set; } = [];
    
    private readonly List<MyMessage> _messagesBackFromLastResponse = [];

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

    public async Task<MyResponse<T>> RunAsync<T>(string message)
    {
        return await RunAsync<T>(new List<MyMessage>
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
        IList<MyMessage> messagesToSend = GetMessagesToSend(messages);

        JsonObject json = BuildPayload(messagesToSend);

        string jsonResponse = await CallProvider(json);

        InternalResponse internalResponse = JsonSerializer.Deserialize<InternalResponse>(jsonResponse)!;

        InternalResponseChoice choice = internalResponse.Choices[0];

        _messagesBackFromLastResponse.Add(choice.Message);
        switch (choice.FinishReason)
        {
            case "stop":
                //Finished!
                MyResponse finalResponse = new(new List<MyMessage>(_messagesBackFromLastResponse));
                _messagesBackFromLastResponse.Clear();
                return finalResponse;
            case "tool_calls":
                //Tool call
                return await ReactToToolCallRequest(messagesToSend, choice.Message);
            default:
                throw new Exception($"Unexcepted finishReason: {choice.FinishReason}");
        }
    }

    public async Task<MyResponse<T>> RunAsync<T>(IList<MyMessage> messages)
    {
        ResponseFormat = BuildResponseFormat<T>();
        MyResponse response = await RunAsync(messages);
        ResponseFormat = null;
        return new MyResponse<T>(response);
    }

    private async Task<MyResponse> ReactToToolCallRequest(IList<MyMessage> messagesToSend, MyMessage toolCallRequestMessageFromLlm)
    {
        foreach (MyMessageToolCallRequest toolCallRequest in toolCallRequestMessageFromLlm.ToolCalls ?? [])
        {
            MyTool? tool = Tools.FirstOrDefault(x => x.Name == toolCallRequest.Function.Name);
            if (tool == null)
            {
                throw new Exception("Requested tool does not exist!");
            }

            messagesToSend.Add(toolCallRequestMessageFromLlm);

            object? toolResult;
            if (!string.IsNullOrWhiteSpace(toolCallRequest.Function.Arguments))
            {
                using JsonDocument json = JsonDocument.Parse(toolCallRequest.Function.Arguments);

                ParameterInfo[] parameterInfos = tool.Delegate.Method.GetParameters();
                List<object> args = [];
                foreach (ParameterInfo parameterInfo in parameterInfos)
                {
                    JsonElement jsonElement = json.RootElement.GetProperty(parameterInfo.Name!);
                    if (parameterInfo.ParameterType == typeof(string))
                    {
                        args.Add(jsonElement.GetString()!);
                    }
                    else if (parameterInfo.ParameterType == typeof(int))
                    {
                        args.Add(jsonElement.GetInt32());
                    }
                    else
                    {
                        throw new Exception("Not yet supported parameter");
                    }
                }

                toolResult = tool.Delegate.DynamicInvoke(args.ToArray());
            }
            else
            {
                toolResult = tool.Delegate.DynamicInvoke();
            }

            MyMessage toolMessage = new()
            {
                Content = toolResult?.ToString()!,
                Role = "tool",
                ToolCallId = toolCallRequest.Id
            };
            _messagesBackFromLastResponse.Add(toolMessage);
            messagesToSend.Add(toolMessage);
        }
        return await RunAsync(messagesToSend);
    }

    private JsonObject BuildResponseFormat<T>()
    {
        Type type = typeof(T);

        Dictionary<string, object> properties = [];
        PropertyInfo[] propertyInfos = type.GetProperties();
        foreach (PropertyInfo propertyInfo in propertyInfos)
        {
            properties.Add(propertyInfo.Name, new JsonObject
            {
                {"type", CSharpTypeToSchemaType(propertyInfo.PropertyType)}
            });
        }
        
        JsonObject root = new()
        {
            {"type", "json_schema"},
            {"json_schema", new JsonObject
            {
                {"name", type.Name},
                {"schema", new JsonObject
                {
                    {"$schema", "https://json-schema.org/draft/2020-12/schema"},
                    {"type", "object"},
                    {"properties", JsonSerializer.SerializeToNode(properties)}
                }}
            } }
        };
        return root;
    }

    private string CSharpTypeToSchemaType(Type type)
    {
        if (type == typeof(String))
        {
            return "string";
        }
        if (type == typeof(Int32))
        {
            return "integer";
        }

        throw new Exception("Not yet supported type");
    }

    private async Task<string> CallProvider(JsonObject json)
    {
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
        return await httpResponse.Content.ReadAsStringAsync();
    }

    private IList<MyMessage> GetMessagesToSend(IList<MyMessage> messages)
    {
        IList<MyMessage> messagesToSend = messages;
        if (!string.IsNullOrWhiteSpace(Instructions) && messagesToSend.All(x=> x.Role != "system"))
        {
            messagesToSend.Insert(0, new MyMessage
            {
                Role = "system",
                Content = Instructions
            }
            );
        }

        return messagesToSend;
    }

    private JsonObject BuildPayload(IList<MyMessage> messagesToSend)
    {
        //Message part of payload
        JsonArray jsonMessages = [];
        foreach (MyMessage message in messagesToSend)
        {
            JsonObject jsonMessage = new()
            {
                {"role", message.Role},
                
            };
            if (!string.IsNullOrWhiteSpace(message.Content))
            {
                jsonMessage.Add("content", message.Content);
            }

            if (message.ToolCalls?.Count > 0)
            {
                jsonMessage.Add("tool_calls", JsonSerializer.SerializeToNode(message.ToolCalls));
            }

            if (!string.IsNullOrWhiteSpace(message.ToolCallId))
            {
                jsonMessage.Add("tool_call_id", message.ToolCallId);
            }

            jsonMessages.Add(jsonMessage);
        }

        JsonObject json = new()
        {
            {"messages", jsonMessages}
        };

        //Structured Output?
        if (ResponseFormat != null)
        {
            json.Add("response_format", ResponseFormat);
        }

        //Tools?
        if (Tools.Count > 0)
        {
            JsonArray tools = [];
            foreach (MyTool tool in Tools)
            {
                ParameterInfo[] parameterInfos = tool.Delegate.Method.GetParameters();
                Dictionary<string, object> parameters = [];
                foreach (ParameterInfo parameterInfo in parameterInfos)
                {
                    parameters.Add(parameterInfo.Name!, new JsonObject
                    {
                        {"type", CSharpTypeToSchemaType(parameterInfo.ParameterType)}
                    });
                }

                tools.Add(new JsonObject
                {
                    {"type", "function"},
                    {"function", new JsonObject
                    {
                        {"description", tool.Description},
                        {"name", tool.Name},
                        {"parameters", new JsonObject
                        {
                            {"type", "object"},
                            {"properties", JsonSerializer.SerializeToNode(parameters)}
                        }}
                    }}
                });
            }

            json.Add("tools", tools);
        }

        return json;
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