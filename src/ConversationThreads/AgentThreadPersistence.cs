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

                RestoreConsole(resumedThread);
                return resumedThread;
            }
        }

        return await agent.CreateSessionAsync();
    }

    private static void RestoreConsole(AgentSession resumedSession)
    {
        //todo: this sample does not work in RC1 - Need answer from Team (https://github.com/microsoft/agent-framework/issues/4140)
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

    public static async Task StoreThreadAsync(AIAgent agent, AgentSession session)
    {
        JsonElement serializedThread = await agent.SerializeSessionAsync(session);
        await File.WriteAllTextAsync(ConversationPath, JsonSerializer.Serialize(serializedThread));
    }
}