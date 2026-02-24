//YouTube video that cover this sample: https://youtu.be/tDQc6lZUbYc

using Microsoft.Agents.AI;
using Microsoft.Agents.AI.AGUI;
using Microsoft.Extensions.AI;
using System.Text;

Console.Clear();
HttpClient httpClient = new HttpClient();
const string serverUrl = "http://localhost:5000";
ConsoleColor textColor = ConsoleColor.White;

AGUIChatClient chatClient = new AGUIChatClient(httpClient, serverUrl);
AIAgent agent = chatClient.AsAIAgent(tools: [AIFunctionFactory.Create(ChangeColor, name: "change_color")]);

//Note that certain Agent Feature do not work in AG-UI (Instructions and Sessions) so need to initialized/maintained manually
List<ChatMessage> messages = [new ChatMessage(ChatRole.System, "You are a nice AI Agent")];

while (true)
{
    Console.Write("> ");
    string message = Console.ReadLine() ?? string.Empty;
    if (message == string.Empty)
    {
        continue;
    }

    messages.Add(new ChatMessage(ChatRole.User, message));

    List<AgentResponseUpdate> updates = [];
    await foreach (AgentResponseUpdate update in agent.RunStreamingAsync(messages))
    {
        updates.Add(update);
        foreach (AIContent content in update.Contents)
        {
            switch (content)
            {
                case TextContent textContent:
                    Console.ForegroundColor = textColor;
                    Console.Write(textContent.Text);
                    break;

                case FunctionCallContent functionCallContent:
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    StringBuilder toolCallDetails = new();
                    toolCallDetails.Append($"[Tool Call: {functionCallContent.Name}");
                    if (functionCallContent.Arguments != null && functionCallContent.Arguments.Any())
                    {
                        toolCallDetails.Append($" (Args: {string.Join(",", functionCallContent.Arguments.Select(x => $"[{x.Key} = {x.Value}]"))}");
                    }

                    toolCallDetails.Append("]");
                    Console.WriteLine(toolCallDetails);
                    Console.ForegroundColor = textColor;
                    break;
                case FunctionResultContent functionResultContent:
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    bool isError = functionResultContent.Exception != null;
                    Console.WriteLine(isError ? $"[Tool Error: {functionResultContent.Exception}]" : $"[Tool Result: {functionResultContent.Result}]");

                    Console.ForegroundColor = textColor;
                    break;

                case ErrorContent errorContent:
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.WriteLine($"[Error: {errorContent.Message}]");
                    Console.ForegroundColor = textColor;
                    break;
            }
        }
    }

    AgentResponse fullResponse = updates.ToAgentResponse();
    messages.AddRange(fullResponse.Messages);

    Console.WriteLine();
    Console.WriteLine();
}

void ChangeColor(ConsoleColor color)
{
    textColor = color;
    Console.ForegroundColor = textColor;
}