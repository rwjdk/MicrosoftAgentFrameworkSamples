//WARNING: This is a playground area for the creator of the Repo to test and tinker. Nothing in this project is as such educational and might not even execute properly
#pragma warning disable OPENAI002
using NAudio.Wave;
using OpenAI.Realtime;
using Shared;
using OpenAI;
using Playground;

Utils.Init("Playground");
Secrets secrets = SecretsManager.GetSecrets();

OpenAIClient openAIClient = new(secrets.OpenAiApiKey);

RealtimeClient realtimeClient = openAIClient.GetRealtimeClient();

const string realtimeModel = "gpt-realtime-mini";
const string inputTranscriptionModel = "gpt-4o-mini-transcribe";
const int sampleRate = 24_000;

CancellationTokenSource cancellationToken = new();
//Include option to press CTRL + C to end program
Console.CancelKeyPress += (_, args) =>
{
    args.Cancel = true;
    cancellationToken.Cancel();
};

Console.WriteLine("Starting voice session...");

using RealtimeSessionClient session = await realtimeClient.StartConversationSessionAsync(realtimeModel);

WaveFormat pcmFormat = new(sampleRate, 16, 1);
ConversationConsole conversationConsole = new();
using StreamingAudioPlayer audioPlayer = new(pcmFormat);
await using MicrophoneStreamer microphone = new(pcmFormat, cancellationToken.Token);

Task receiveTask = ReceiveUpdatesAsync(session, audioPlayer, conversationConsole, cancellationToken.Token);
Task microphoneUploadTask = UploadMicrophoneAudioAsync(session, microphone, cancellationToken.Token);

await session.ConfigureConversationSessionAsync(BuildSessionOptions(), cancellationToken.Token);

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
catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
{
}
finally
{
    cancellationToken.Cancel();
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