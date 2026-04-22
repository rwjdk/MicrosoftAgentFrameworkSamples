#pragma warning disable OPENAI002
using OpenAI;
using OpenAI.Realtime;
using Shared;
using System.ClientModel;
using OpenAI.RealtimeAPI;

Utils.Init("Speech-to-Speech");
Secrets secrets = SecretsManager.GetSecrets();

//OpenAIClient client = new(secrets.OpenAiApiKey);
OpenAIClient client = new(new ApiKeyCredential(secrets.AzureOpenAiKey), new OpenAIClientOptions
{
    Endpoint = new Uri(secrets.AzureOpenAiEndpoint+"/openai/v1") //Note that AZURE OpenAI only work via the OpenAI Client + Endpoint for Realtime
});

RealtimeClient realtimeClient = client.GetRealtimeClient();

const string realtimeModel = "gpt-realtime-mini"; //Bit expensive: 10$ per 1M audio input | 20$ per 1M audio output (32$/64$ for non-mini https://developers.openai.com/api/docs/pricing)
const string inputTranscriptionModel = "gpt-4o-mini-transcribe"; //optional

CancellationTokenSource cancellationToken = new();
Console.CancelKeyPress += (_, args) => //Include option to press CTRL + C to end program
{
    args.Cancel = true;
    cancellationToken.Cancel();
};

using RealtimeSessionClient session = await realtimeClient.StartConversationSessionAsync(realtimeModel);
await session.ConfigureConversationSessionAsync(new RealtimeConversationSessionOptions
{
    Instructions = """
                   You are a voice assistant.
                   Keep responses brief, clear, and conversational.
                   If the user interrupts, stop your current thought and respond to the newest thing they said.
                   """,
    AudioOptions = new RealtimeConversationSessionAudioOptions
    {
        InputAudioOptions = new RealtimeConversationSessionInputAudioOptions
        {
            AudioTranscriptionOptions = new RealtimeAudioTranscriptionOptions //Leave out if you do not want transcripts
            {
                Model = inputTranscriptionModel,
            },
            NoiseReduction = new RealtimeNoiseReduction(RealtimeNoiseReductionKind.NearField),
            TurnDetection = new RealtimeServerVadTurnDetection
            {
                DetectionThreshold = 0.5f,
                PrefixPadding = TimeSpan.FromMilliseconds(300), //How much prefix sound to add prior to voice detection
                SilenceDuration = TimeSpan.FromMilliseconds(550), //How long there need to be silence before AI begin to answer
                IdleTimeout = TimeSpan.FromSeconds(10), //How long AI wait before wanting to follow up (5-30 sec. is allowed)
                InterruptResponseEnabled = true, //If you can interrupt the AI while it is answering (does not happen instantly though)
            },
        },
        OutputAudioOptions = new RealtimeConversationSessionOutputAudioOptions
        {
            Voice = RealtimeVoice.Marin,
            Speed = 1.0f,
        },
    },
    OutputModalities =
    {
        RealtimeOutputModality.Audio
    },
});

using StreamingAudioPlayer audioPlayer = new();
await using MicrophoneStreamer microphone = new(cancellationToken.Token);

//Start two Tasks... One for Uploading audio and one for receiving updates back
Task microphoneUploadTask = UploadMicrophoneAudioAsync(session, microphone, cancellationToken.Token);
Task receiveTask = ReceiveUpdatesAsync(session, audioPlayer, cancellationToken.Token);

try
{
    try
    {
        microphone.Start();
    }
    catch (Exception ex)
    {
        throw new InvalidOperationException("Unable to start microphone capture.Check that a recording device is available and not in exclusive use.", ex);
    }
    await receiveTask;
}
catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
{
    //Ignore Cancel-exception
}
finally
{
    cancellationToken.Cancel();
    microphone.Stop();
    audioPlayer.Clear();
    try
    {
        await microphoneUploadTask; //Complete the microphone task for good measure
    }
    catch (OperationCanceledException e)
    {
        //Ignore Cancel-exception
    }
}

static async Task UploadMicrophoneAudioAsync(RealtimeSessionClient session, MicrophoneStreamer microphone, CancellationToken cancellationToken)
{
    await foreach (byte[] audioChunk in microphone.ReadAllAsync(cancellationToken))
    {
        await session.SendInputAudioAsync(BinaryData.FromBytes(audioChunk), cancellationToken);
    }
}

static async Task ReceiveUpdatesAsync(RealtimeSessionClient session, StreamingAudioPlayer audioPlayer, CancellationToken cancellationToken)
{
    await foreach (RealtimeServerUpdate update in session.ReceiveUpdatesAsync(cancellationToken))
    {
        switch (update)
        {
            case RealtimeServerUpdateSessionCreated:
                Utils.Gray("Session Created:");
                Utils.Gray("- Speak naturally and wait for the AI to answer back.");
                Utils.Gray("- Press Ctrl+C to stop.");
                break;
            case RealtimeServerUpdateInputAudioBufferSpeechStarted:
                Utils.Gray("[Listening...]");
                break;
            case RealtimeServerUpdateInputAudioBufferSpeechStopped:
                Utils.Gray("[Thinking...]");
                break;
            case RealtimeServerUpdateConversationItemInputAudioTranscriptionCompleted completed:
                Utils.Gray($"[You]: {completed.Transcript}");
                break;
            case RealtimeServerUpdateResponseOutputAudioTranscriptDone done:
                Utils.Gray($"[AI]: {done.Transcript}");
                break;
            case RealtimeServerUpdateResponseOutputAudioDelta delta:
                audioPlayer.Enqueue(delta.ItemId, delta.Delta.ToArray());
                break;
            case RealtimeServerUpdateOutputAudioBufferCleared:
                audioPlayer.Clear();
                Console.WriteLine();
                break;
            case RealtimeServerUpdateResponseDone responseDone:
                audioPlayer.FlushPendingAudio();
                RealtimeResponse response = responseDone.Response;
                if (response.Status != RealtimeResponseStatus.Completed)
                {
                    string message = response.StatusDetails?.Error?.Message ?? response.StatusDetails?.Reason?.ToString() ?? "Unknown response issue.";
                    Utils.Red($"[Status: {message}]");
                }
                break;
            case RealtimeServerUpdateError error:
                Utils.Red($"[error:{error.Error.Code}] {error.Error.Message}");
                break;
        }
    }
}