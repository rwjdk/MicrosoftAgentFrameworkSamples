using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using System.Text.Json;

namespace ConversationThreads;

public static class AgentThreadPersistence
{
    private static string ConversationPath => Path.Combine(Path.GetTempPath(), "conversation.json");

    public static async Task<AgentSession> ResumeChatIfRequestedAsync(ChatClientAgent agent)
    {
        if (File.Exists(ConversationPath))
        {
            Console.Write("Restore previous conversation? (Y/N): ");
            ConsoleKeyInfo key = Console.ReadKey();
            Console.Clear();
            if (key.Key == ConsoleKey.Y)
            {
                JsonElement jsonElement = JsonSerializer.Deserialize<JsonElement>(await File.ReadAllTextAsync(ConversationPath));
                AgentSession resumedThread = await agent.DeserializeSessionAsync(jsonElement);

                await RestoreConsole(resumedThread);
                return resumedThread;
            }
        }

        return await agent.CreateSessionAsync();
    }

    private static async Task RestoreConsole(AgentSession resumedSession)
    {
        ChatClientAgentSession chatClientAgentThread = (ChatClientAgentSession)resumedSession;
        if (chatClientAgentThread.ChatHistoryProvider != null)
        {
            IList<ChatMessage>? messages = resumedSession.GetService<IList<ChatMessage>>();
            foreach (ChatMessage message in messages!)
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

    public static async Task StoreThreadAsync(AIAgent agent, AgentSession session)
    {
        JsonElement serializedThread = await agent.SerializeSessionAsync(session);
        await File.WriteAllTextAsync(ConversationPath, JsonSerializer.Serialize(serializedThread));
    }
}