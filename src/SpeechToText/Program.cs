using Azure.AI.OpenAI;
using NAudio.Utils;
using NAudio.Wave;
using OpenAI.Audio;
using Shared;
using System.ClientModel;
using Microsoft.Agents.AI;
using OpenAI;
using OpenAI.Chat;

Console.Clear();
Secrets secrets = SecretManager.GetSecrets();

AzureOpenAIClient azureOpenAIClient = new AzureOpenAIClient(new Uri(secrets.AzureOpenAiEndpoint), new ApiKeyCredential(secrets.AzureOpenAiKey));
AudioClient audioClient = azureOpenAIClient.GetAudioClient("whisper");

var agent = azureOpenAIClient
    .GetChatClient(secrets.ChatDeploymentName)
    .AsAIAgent(instructions: "You are a Friendly AI Bot, answering questions");

AgentSession agentSession = await agent.GetNewSessionAsync();

while (true)
{
    Console.WriteLine("Press any key to start recording...");
    Console.ReadKey();

    //Record the Audio
    using MemoryStream audioStream = RecordAudio();

    //Turn Audio into Text
    ClientResult<AudioTranscription> result = await audioClient.TranscribeAudioAsync(audioStream, "audio.wav");

    string questionFromAudio = result.Value.Text;
    Console.WriteLine($"> {questionFromAudio}");

    AgentResponse response = await agent.RunAsync(questionFromAudio, agentSession);
    Console.WriteLine(response);

    Utils.Separator();
}

MemoryStream RecordAudio()
{
    MemoryStream stream = new();
    using WaveInEvent waveIn = new();
    waveIn.WaveFormat = new WaveFormat(16000, 16, 1);
    WaveFileWriter writer = new(new IgnoreDisposeStream(stream), waveIn.WaveFormat);

    waveIn.DataAvailable += (_, args) => { writer.Write(args.Buffer, 0, args.BytesRecorded); };
    waveIn.StartRecording();

    Console.WriteLine("Recording... Press any key to stop");
    Console.ReadKey();

    waveIn.StopRecording();
    stream.Position = 0;
    return stream;
}