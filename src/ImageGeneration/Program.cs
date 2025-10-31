//YouTube video that cover this sample https://youtu.be/F8BxvnpWJ9s
//NB: This samples is not as such related to Microsoft Agent Framework, but output can of cause be used

using OpenAI.Images;
using Shared;
using System.ClientModel;
using Azure.AI.OpenAI;
using OpenAI;

#pragma warning disable OPENAI001

Console.Clear();
Configuration configuration = ConfigurationManager.GetConfiguration();
AzureOpenAIClient client = new(new Uri(configuration.AzureOpenAiEndpoint), new ApiKeyCredential(configuration.AzureOpenAiKey));
//OpenAIClient client = new(configuration.OpenAiApiKey);

ImageClient imageClient = client.GetImageClient("gpt-image-1");
ClientResult<GeneratedImage> image = await imageClient.GenerateImageAsync("A Tiger in a jungle with a party-hat", new ImageGenerationOptions
{
    Background = GeneratedImageBackground.Auto,
    Quality = GeneratedImageQuality.Auto,
    Size = GeneratedImageSize.W1024xH1024,
    OutputFileFormat = GeneratedImageFileFormat.Png,
});
byte[] bytes = image.Value.ImageBytes.ToArray();
string path = Path.Combine(Path.GetTempPath(), $"image-{Guid.NewGuid():N}.png");
File.WriteAllBytes(path, bytes);

await Task.Factory.StartNew(() =>
{
    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
    {
        FileName = path,
        UseShellExecute = true
    });
});