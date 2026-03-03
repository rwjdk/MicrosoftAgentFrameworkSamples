using AgentFrameworkToolkit;
using Azure.AI.OpenAI;
using JetBrains.Annotations;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Shared.Extensions;
using System.ClientModel;
using System.ClientModel.Primitives;
using System.Text;
using System.Text.Json;

namespace Shared;

[PublicAPI]
public static class Utils
{
    public static void Red(Exception e)
    {
        Red(e.ToString());
    }

    public static void Red(string text)
    {
        WriteLine(text, ConsoleColor.Red);
    }

    public static void Yellow(object text)
    {
        WriteLine(text.ToString()!, ConsoleColor.Yellow);
    }

    public static void Yellow(string text)
    {
        WriteLine(text, ConsoleColor.Yellow);
    }

    public static void Gray(string text)
    {
        WriteLine(text, ConsoleColor.DarkGray);
    }

    public static void Green(string text)
    {
        WriteLine(text, ConsoleColor.Green);
    }

    public static void WriteLine(string text, ConsoleColor color)
    {
        ConsoleColor orgColor = Console.ForegroundColor;
        try
        {
            Console.ForegroundColor = color;
            Console.WriteLine(text);
        }
        finally
        {
            Console.ForegroundColor = orgColor;
        }
    }

    public static void Separator()
    {
        Console.WriteLine();
        WriteLine("".PadLeft(Console.WindowWidth, '-'), ConsoleColor.Gray);
        Console.WriteLine();
    }

    public static void Init(string? title = null)
    {
        Console.Clear();
        Console.OutputEncoding = Encoding.UTF8;
        if (!string.IsNullOrWhiteSpace(title))
        {
            Gray($"--- {title} ---");
        }
    }

    public static async Task RunChatLoopWithSession(AIAgent agent)
    {
        AgentSession session = await agent.CreateSessionAsync();
        while (true)
        {
            Console.Write("> ");
            string message = Console.ReadLine() ?? "";
            if (message.Equals("/new", StringComparison.CurrentCultureIgnoreCase))
            {
                session = await agent.CreateSessionAsync();
                Console.Clear();
                continue;
            }
            AgentResponse response = await agent.RunAsync(message, session);
            Console.WriteLine(response);
            Console.WriteLine();
            response.Usage.OutputAsInformation();
            Utils.Separator();
        }
        // ReSharper disable once FunctionNeverReturns
    }

    public static async ValueTask<object?> ToolCallingMiddleware(AIAgent callingAgent, FunctionInvocationContext context, Func<FunctionInvocationContext, CancellationToken, ValueTask<object?>> next, CancellationToken cancellationToken)
    {
        StringBuilder functionCallDetails = new();
        functionCallDetails.Append($"- Tool Call: '{context.Function.Name}'");
        if (context.Arguments.Count > 0)
        {
            functionCallDetails.Append($" (Args: {string.Join(",", context.Arguments.Select(x => $"[{x.Key} = {x.Value}]"))}");
        }

        Gray(functionCallDetails.ToString());

        return await next(context, cancellationToken);
    }

    public static AzureOpenAIClient GetAzureOpenAIClient(bool showRawCall, RawCallOptions? rawCall = null)
    {
        (Uri endpoint, ApiKeyCredential apiKey) = SecretsManager.GetAzureOpenAICredentials();
        if (!showRawCall)
        {
            return new AzureOpenAIClient(endpoint, apiKey);
        }

        return new AzureOpenAIClient(endpoint, apiKey, new AzureOpenAIClientOptions
        {
            Transport = new HttpClientPipelineTransport(new HttpClient(new CustomClientHttpHandler(rawCall ?? new RawCallOptions())))
        });

    }

    public class RawCallOptions
    {
        public bool ShowUrl { get; set; }
        public bool ShowRequest { get; set; } = true;
        public bool ShowResponse { get; set; }
    }

    class CustomClientHttpHandler(RawCallOptions rawCallOptions) : HttpClientHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            string requestString = await request.Content?.ReadAsStringAsync(cancellationToken)!;
            if (rawCallOptions.ShowUrl)
            {
                Utils.Green($"Raw Request ({request.RequestUri})");
            }

            if (rawCallOptions.ShowRequest)
            {
                Utils.Gray(MakePretty(requestString));
                Utils.Separator();
            }

            var response = await base.SendAsync(request, cancellationToken);

            if (rawCallOptions.ShowResponse)
            {
                string responseString = await response.Content.ReadAsStringAsync(cancellationToken);
                Utils.Green("Raw Response");
                Utils.Gray(MakePretty(responseString));
                Utils.Separator();
            }

            return response;
        }

        private string MakePretty(string input)
        {
            try
            {
                var jsonElement = JsonSerializer.Deserialize<JsonElement>(input);
                return JsonSerializer.Serialize(jsonElement, new JsonSerializerOptions { WriteIndented = true });
            }
            catch
            {
                return input;
            }
        }
    }


}