//WARNING: This is a playground area for the creator of the Repo to test and tinker. Nothing in this project is as such educational and might not even execute properly
#pragma warning disable OPENAI002
using NAudio.Wave;
using OpenAI.Realtime;
using Shared;
using System.Text;
using System.Threading.Channels;

TryInitConsole("Playground");

const string model = "gpt-realtime-mini";
const string inputTranscriptionModel = "gpt-4o-mini-transcribe";
const int sampleRate = 24_000;

Secrets secrets = SecretsManager.GetSecrets();

using CancellationTokenSource cts = new();
Console.CancelKeyPress += (_, args) =>
{
    args.Cancel = true;
    cts.Cancel();
};

WaveFormat pcmFormat = new(sampleRate, 16, 1);
ConversationConsole conversationConsole = new();
RealtimeClient realtimeClient = new(secrets.OpenAiApiKey);

Console.WriteLine("Starting realtime voice session...");

using RealtimeSessionClient session = await realtimeClient.StartConversationSessionAsync(
    model,
    new RealtimeSessionClientOptions(),
    cts.Token);

using StreamingAudioPlayer audioPlayer = new(pcmFormat);
await using MicrophoneStreamer microphone = new(pcmFormat, cts.Token);

Task receiveTask = ReceiveUpdatesAsync(session, audioPlayer, conversationConsole, cts.Token);
Task microphoneUploadTask = UploadMicrophoneAudioAsync(session, microphone, cts.Token);

await session.ConfigureConversationSessionAsync(BuildSessionOptions(), cts.Token);

Console.WriteLine("Realtime conversation is live.");
Console.WriteLine("Speak naturally and wait for the AI to answer back.");
Console.WriteLine("Use headphones if you can to avoid speaker feedback.");
Console.WriteLine("Press Ctrl+C to stop.");
Console.WriteLine();

try
{
    try
    {
        microphone.Start();
    }
    catch (Exception ex)
    {
        throw new InvalidOperationException(
            $"Unable to start microphone capture at {sampleRate / 1000.0:0} kHz mono PCM. Check that a recording device is available and not in exclusive use.",
            ex);
    }

    await receiveTask;
}
catch (OperationCanceledException) when (cts.IsCancellationRequested)
{
}
finally
{
    cts.Cancel();
    microphone.Stop();
    await microphone.DisposeAsync();
    audioPlayer.Clear();

    try
    {
        await microphoneUploadTask;
    }
    catch (OperationCanceledException)
    {
    }
}

static RealtimeConversationSessionOptions BuildSessionOptions()
{
    return new RealtimeConversationSessionOptions
    {
        Instructions =
            """
            You are a low-latency voice assistant in a console playground.
            Keep responses brief, clear, and conversational.
            Ask a short follow-up question when it helps keep the conversation moving.
            If the user interrupts, stop your current thought and respond to the newest thing they said.
            """,
        MaxOutputTokenCount = new RealtimeMaxOutputTokenCount(350),
        AudioOptions = new RealtimeConversationSessionAudioOptions
        {
            InputAudioOptions = new RealtimeConversationSessionInputAudioOptions
            {
                AudioFormat = CreatePcmFormat(),
                AudioTranscriptionOptions = new RealtimeAudioTranscriptionOptions
                {
                    Model = inputTranscriptionModel,
                },
                NoiseReduction = new RealtimeNoiseReduction(RealtimeNoiseReductionKind.NearField),
                TurnDetection = new RealtimeServerVadTurnDetection
                {
                    DetectionThreshold = 0.5f,
                    PrefixPadding = TimeSpan.FromMilliseconds(300),
                    SilenceDuration = TimeSpan.FromMilliseconds(550),
                    IdleTimeout = TimeSpan.FromSeconds(6),
                    CreateResponseEnabled = true,
                    InterruptResponseEnabled = true,
                },
            },
            OutputAudioOptions = new RealtimeConversationSessionOutputAudioOptions
            {
                AudioFormat = CreatePcmFormat(),
                Voice = RealtimeVoice.Marin,
                Speed = 1.0f,
            },
        },
        OutputModalities =
        {
            RealtimeOutputModality.Audio,
        },
    };
}

static RealtimePcmAudioFormat CreatePcmFormat() => new();

static void TryInitConsole(string title)
{
    try
    {
        Console.Clear();
    }
    catch (IOException)
    {
    }

    Console.OutputEncoding = Encoding.UTF8;
    Console.WriteLine($"--- {title} ---");
    Console.WriteLine();
}

static async Task UploadMicrophoneAudioAsync(
    RealtimeSessionClient session,
    MicrophoneStreamer microphone,
    CancellationToken cancellationToken)
{
    await foreach (byte[] audioChunk in microphone.ReadAllAsync(cancellationToken))
    {
        await session.SendInputAudioAsync(BinaryData.FromBytes(audioChunk), cancellationToken);
    }
}

static async Task ReceiveUpdatesAsync(
    RealtimeSessionClient session,
    StreamingAudioPlayer audioPlayer,
    ConversationConsole conversationConsole,
    CancellationToken cancellationToken)
{
    await foreach (RealtimeServerUpdate update in session.ReceiveUpdatesAsync(cancellationToken))
    {
        switch (update)
        {
            case RealtimeServerUpdateSessionCreated:
                conversationConsole.PrintStatus("Session connected.");
                break;

            case RealtimeServerUpdateSessionUpdated:
                conversationConsole.PrintStatus("Session configured for speech-to-speech.");
                break;

            case RealtimeServerUpdateInputAudioBufferSpeechStarted:
                audioPlayer.Clear();
                conversationConsole.NotifyListening();
                break;

            case RealtimeServerUpdateInputAudioBufferSpeechStopped:
                conversationConsole.NotifyThinking();
                break;

            case RealtimeServerUpdateConversationItemInputAudioTranscriptionCompleted completed:
                conversationConsole.PrintUserTranscript(completed.Transcript);
                break;

            case RealtimeServerUpdateResponseOutputAudioTranscriptDelta delta:
                conversationConsole.AppendAssistantTranscript(delta.ItemId, delta.Delta);
                break;

            case RealtimeServerUpdateResponseOutputAudioTranscriptDone done:
                conversationConsole.CompleteAssistantTranscript(done.ItemId, done.Transcript);
                break;

            case RealtimeServerUpdateResponseOutputAudioDelta delta:
                audioPlayer.Enqueue(delta.Delta.ToArray());
                break;

            case RealtimeServerUpdateOutputAudioBufferCleared:
                audioPlayer.Clear();
                conversationConsole.ResetAssistantLine();
                break;

            case RealtimeServerUpdateResponseDone responseDone:
                conversationConsole.FinishResponse(responseDone.Response);
                break;

            case RealtimeServerUpdateError error:
                conversationConsole.PrintError(error.Error);
                break;
        }
    }
}

sealed class StreamingAudioPlayer : IDisposable
{
    private readonly BufferedWaveProvider _bufferedWaveProvider;
    private readonly IWavePlayer _player;
    private readonly object _syncRoot = new();

    public StreamingAudioPlayer(WaveFormat waveFormat)
    {
        _bufferedWaveProvider = new BufferedWaveProvider(waveFormat)
        {
            BufferDuration = TimeSpan.FromSeconds(20),
            DiscardOnBufferOverflow = true,
        };

        _player = new WaveOutEvent();
        _player.Init(_bufferedWaveProvider);
        _player.Play();
    }

    public void Enqueue(byte[] audioBytes)
    {
        if (audioBytes.Length == 0)
        {
            return;
        }

        lock (_syncRoot)
        {
            _bufferedWaveProvider.AddSamples(audioBytes, 0, audioBytes.Length);
        }
    }

    public void Clear()
    {
        lock (_syncRoot)
        {
            _bufferedWaveProvider.ClearBuffer();
        }
    }

    public void Dispose()
    {
        _player.Dispose();
    }
}

sealed class MicrophoneStreamer : IAsyncDisposable
{
    private readonly WaveInEvent _waveIn;
    private readonly Channel<byte[]> _audioChannel;
    private readonly CancellationTokenRegistration _cancellationRegistration;
    private bool _isDisposed;

    public MicrophoneStreamer(WaveFormat waveFormat, CancellationToken cancellationToken)
    {
        _audioChannel = Channel.CreateUnbounded<byte[]>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = true,
        });

        _waveIn = new WaveInEvent
        {
            BufferMilliseconds = 100,
            NumberOfBuffers = 3,
            WaveFormat = waveFormat,
        };

        _waveIn.DataAvailable += HandleDataAvailable;
        _waveIn.RecordingStopped += (_, args) =>
        {
            if (args.Exception is not null)
            {
                _audioChannel.Writer.TryComplete(args.Exception);
                return;
            }

            _audioChannel.Writer.TryComplete();
        };

        _cancellationRegistration = cancellationToken.Register(Stop);
    }

    public void Start() => _waveIn.StartRecording();

    public void Stop()
    {
        if (_isDisposed)
        {
            return;
        }

        try
        {
            _waveIn.StopRecording();
        }
        catch
        {
            _audioChannel.Writer.TryComplete();
        }
    }

    public IAsyncEnumerable<byte[]> ReadAllAsync(CancellationToken cancellationToken)
        => _audioChannel.Reader.ReadAllAsync(cancellationToken);

    public ValueTask DisposeAsync()
    {
        if (_isDisposed)
        {
            return ValueTask.CompletedTask;
        }

        _isDisposed = true;
        _cancellationRegistration.Dispose();
        _waveIn.DataAvailable -= HandleDataAvailable;
        _waveIn.Dispose();
        _audioChannel.Writer.TryComplete();
        return ValueTask.CompletedTask;
    }

    private void HandleDataAvailable(object? sender, WaveInEventArgs args)
    {
        if (args.BytesRecorded <= 0)
        {
            return;
        }

        byte[] chunk = new byte[args.BytesRecorded];
        Buffer.BlockCopy(args.Buffer, 0, chunk, 0, args.BytesRecorded);
        _audioChannel.Writer.TryWrite(chunk);
    }
}

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

            if (response.Status is not null
                && response.Status.ToString() is string status
                && !string.Equals(status, RealtimeResponseStatus.Completed.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                string message = response.StatusDetails?.Error?.Message ?? response.StatusDetails?.Reason?.ToString() ?? "Unknown response issue.";
                Console.WriteLine($"[response:{status}] {message}");
            }
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
