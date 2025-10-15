using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using System.Text.Json;

namespace ConversationThreads;

public static class AgentThreadPersistence
{
    private static string ConversationPath => Path.Combine(Path.GetTempPath(), "conversation.json");

    public static async Task<AgentThread> ResumeChatIfRequestedAsync(ChatClientAgent agent)
    {
        if (File.Exists(ConversationPath))
        {
            Console.Write("Restore previous conversation? (Y/N): ");
            ConsoleKeyInfo key = Console.ReadKey();
            Console.Clear();
            if (key.Key == ConsoleKey.Y)
            {
                JsonElement jsonElement = JsonSerializer.Deserialize<JsonElement>(await File.ReadAllTextAsync(ConversationPath));
                AgentThread resumedThread = agent.DeserializeThread(jsonElement);

                await RestoreConsole(resumedThread);
                return resumedThread;
            }
        }

        return agent.GetNewThread();
    }

    private static async Task RestoreConsole(AgentThread resumedThread)
    {
        ChatClientAgentThread chatClientAgentThread = (ChatClientAgentThread)resumedThread;
        if (chatClientAgentThread.MessageStore != null)
        {
            IEnumerable<ChatMessage> messages = await chatClientAgentThread.MessageStore.GetMessagesAsync();
            foreach (ChatMessage message in messages)
            {
                if (message.Role == ChatRole.User)
                {
                    Console.WriteLine($"> {message.Text}");
                }
                else if (message.Role == ChatRole.Assistant)
                {
                    Console.WriteLine($"{message.Text}");
                    Console.WriteLine();
                    Console.WriteLine(string.Empty.PadLeft(50, '*'));
                    Console.WriteLine();
                }
            }
        }
    }

    public static async Task StoreThreadAsync(AgentThread thread)
    {
        JsonElement serializedThread = thread.Serialize();
        await File.WriteAllTextAsync(ConversationPath, JsonSerializer.Serialize(serializedThread));
    }
}