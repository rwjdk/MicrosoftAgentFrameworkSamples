//WARNING: This is a playground area for the creator of the Repo to test and tinker. Nothing in this project is as such educational and might not even execute properly

//Notes
//- Microsoft.Agents.AI.Hosting.AgentCatalog TODO: Guess this is something to be used in AI Foundry

#pragma warning disable OPENAI001
using A2A;
using Azure.AI.OpenAI;
using CSCore;
using CSCore.Codecs;
using CSCore.SoundOut;
using GenerativeAI;
using GenerativeAI.Microsoft;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI;
using OpenAI.Audio;
using OpenAI.Responses;
using Shared;
using System.ClientModel;
using System.ClientModel.Primitives;
using System.IO;
using System.Text;
using System.Text.Json;
using CSCore;
using CSCore.Codecs;
using CSCore.Codecs.MP3;
using CSCore.SoundOut;
using CSCore.Codecs;

Console.WriteLine("");
Console.Clear();

Configuration configuration = ConfigurationManager.GetConfiguration();

OpenAIClient openAiClient = new OpenAIClient(configuration.OpenAiApiKey);
AzureOpenAIClient azureOpenAIClient = new AzureOpenAIClient(new Uri(configuration.AzureOpenAiEndpoint), new ApiKeyCredential(configuration.AzureOpenAiKey));

/* Pricing (as of 1st of December 2025)
 * - gpt-4o-mini-tts    $0.015 / minute
 * - tts:               15 USD / 1 Million Chars
 */

AudioClient audioClient = openAiClient.GetAudioClient("gpt-4o-mini-tts");
GeneratedSpeechVoice voice = new GeneratedSpeechVoice("nova"); //nova, shimmer, echo, onyx, fable, alloy'.
string text = "Hi! Welcome to this video about OpenAI's AudioClient. I'm an AI speaking the words Rasmus entered in his program";
ClientResult<BinaryData> result = audioClient.GenerateSpeech(text, voice, new SpeechGenerationOptions
{
    Instructions = "Speak like a little old lady", //Does not do anything with 'tts', and on 'gpt-4o-mini-tts' it have effect, but nothing special
    ResponseFormat = new GeneratedSpeechFormat("mp3"), //mp3, opus, aac, flac, wav and pcm
    //SpeedRatio = 1 //Speed of the voice
});

byte[] buffer = result.Value.ToArray();
File.WriteAllBytes("X:\\test.mp3", buffer);

byte[] bytes = result.Value.ToArray();
MemoryStream memoryStream = new(bytes);

IWaveSource waveSource = new Mp3MediafoundationDecoder(memoryStream);
ISoundOut soundOut = new WasapiOut();
soundOut.Initialize(waveSource);
soundOut.Play();


Console.WriteLine();

//await AzureOpenAiFoundry.Run(configuration);
//await FileTool.Run(configuration);
//await CodeTool.Run(configuration);
//await ReasoningSummary.Run(configuration);
//await CodexSpecialModels.Run(configuration);
//await SpaceNewsWebSearch.Run(configuration);
//await ResumeConversation.Run(configuration);
//await AzureOpenAiCodex.Run(configuration);

static string GetWeather(string city)
{
    return "It is sunny and 19 degrees today";
}