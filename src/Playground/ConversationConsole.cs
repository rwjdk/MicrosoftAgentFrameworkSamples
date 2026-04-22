using System.Text;
using OpenAI.Realtime;
#pragma warning disable OPENAI002

sealed class ConversationConsole
{
    private readonly Lock _consoleLock = new();
    private readonly HashSet<string> _assistantLinesStarted = [];
    private readonly Dictionary<string, StringBuilder> _assistantTranscripts = [];
    private bool _assistantLineOpen;

    public void PrintStatus(string text)
    {
        lock (_consoleLock)
        {
            ResetAssistantLineUnsafe();
            Console.WriteLine($"[status] {text}");
        }
    }

    public void NotifyListening()
    {
        lock (_consoleLock)
        {
            ResetAssistantLineUnsafe();
            Console.WriteLine("[listening]");
        }
    }

    public void NotifyThinking()
    {
        lock (_consoleLock)
        {
            ResetAssistantLineUnsafe();
            Console.WriteLine("[thinking]");
        }
    }

    public void PrintUserTranscript(string transcript)
    {
        if (string.IsNullOrWhiteSpace(transcript))
        {
            return;
        }

        lock (_consoleLock)
        {
            ResetAssistantLineUnsafe();
            Console.WriteLine($"You: {transcript}");
        }
    }

    public void AppendAssistantTranscript(string itemId, string delta)
    {
        if (string.IsNullOrEmpty(delta))
        {
            return;
        }

        lock (_consoleLock)
        {
            if (!_assistantLinesStarted.Add(itemId))
            {
                Console.Write(delta);
            }
            else
            {
                ResetAssistantLineUnsafe();
                Console.Write("AI: ");
                Console.Write(delta);
                _assistantLineOpen = true;
            }

            if (!_assistantTranscripts.TryGetValue(itemId, out StringBuilder? transcript))
            {
                transcript = new StringBuilder();
                _assistantTranscripts[itemId] = transcript;
            }

            transcript.Append(delta);
        }
    }

    public void CompleteAssistantTranscript(string itemId, string transcript)
    {
        lock (_consoleLock)
        {
            if (!_assistantTranscripts.TryGetValue(itemId, out StringBuilder? builder))
            {
                builder = new StringBuilder();
                _assistantTranscripts[itemId] = builder;
            }

            if (builder.Length == 0 && !string.IsNullOrWhiteSpace(transcript))
            {
                ResetAssistantLineUnsafe();
                Console.WriteLine($"AI: {transcript}");
                _assistantLineOpen = false;
            }
            else if (_assistantLineOpen)
            {
                Console.WriteLine();
                _assistantLineOpen = false;
            }
        }
    }

    public void FinishResponse(RealtimeResponse response)
    {
        lock (_consoleLock)
        {
            ResetAssistantLineUnsafe();


        }
    }

    public void ResetAssistantLine()
    {
        lock (_consoleLock)
        {
            ResetAssistantLineUnsafe();
        }
    }

    public void PrintError(RealtimeError error)
    {
        lock (_consoleLock)
        {
            ResetAssistantLineUnsafe();
            string code = string.IsNullOrWhiteSpace(error.Code) ? error.Kind : error.Code;
            Console.WriteLine($"[error:{code}] {error.Message}");
        }
    }

    private void ResetAssistantLineUnsafe()
    {
        if (_assistantLineOpen)
        {
            Console.WriteLine();
            _assistantLineOpen = false;
        }
    }
}