using Azure.AI.OpenAI;
using NAudio.Wave;
using OpenAI;
using OpenAI.Audio;
using Shared;
using System.ClientModel;

Console.Clear();
Secrets secrets = SecretManager.GetConfiguration();

//OpenAIClient openAiClient = new OpenAIClient(secrets.OpenAiApiKey);
AzureOpenAIClient azureOpenAIClient = new AzureOpenAIClient(new Uri(secrets.AzureOpenAiEndpoint), new ApiKeyCredential(secrets.AzureOpenAiKey));

/* Pricing (as of 1st of December 2025)
 * - gpt-4o-mini-tts    ~0.015 USD / minute
 * - tts:                15 USD / 1 Million Chars
 * - tts HD:             30 USD / 1 Million Chars
 */

AudioClient audioClient = azureOpenAIClient.GetAudioClient("tts");

GeneratedSpeechVoice voice = new GeneratedSpeechVoice("shimmer"); //nova, shimmer, echo, onyx, fable, alloy'.
string text = "Hi! Welcome to this video about OpenAI's AudioClient. I'm an AI speaking the words Rasmus entered in his program";
ClientResult<BinaryData> result = audioClient.GenerateSpeech(text, voice, new SpeechGenerationOptions
{
#pragma warning disable OPENAI001
    //Instructions = "Speak like a pirate", //Does not do anything with 'tts', and on 'gpt-4o-mini-tts' it have effect, but nothing special
#pragma warning restore OPENAI001

    ResponseFormat = new GeneratedSpeechFormat("mp3"), //mp3, opus, aac, flac, wav and pcm

    SpeedRatio = 1 //Speed of the voice
});

byte[] bytes = result.Value.ToArray();

//Save to Disk
File.WriteAllBytes(Path.Combine(Path.GetTempPath(), "test.mp3"), bytes);

//Play directly (NAudio nuget package (Windows Only))
WaveStream waveStream = new Mp3FileReader(new MemoryStream(bytes));
IWavePlayer player = new WaveOutEvent();
player.Init(waveStream);
player.Play();

Console.WriteLine("Playing audio. Press Enter to exit...");
Console.ReadLine();