//YouTube video that cover this sample https://youtu.be/F8BxvnpWJ9s
//NB: This samples is not as such related to Microsoft Agent Framework, but output can of cause be used

using OpenAI.Images;
using Shared;
using System.ClientModel;
using OpenAI;

#pragma warning disable OPENAI001

Console.Clear();
Configuration configuration = ConfigurationManager.GetConfiguration();
OpenAIClient client = new(new ApiKeyCredential(configuration.XAiGrokApiKey), new OpenAIClientOptions
{
    Endpoint = new Uri("https://api.x.ai/v1")
});

ImageClient imageClient = client.GetImageClient("grok-2-image");
ClientResult<GeneratedImage> image = await imageClient.GenerateImageAsync("A Tiger in a jungle with a party-hat", new ImageGenerationOptions
{
    Background = GeneratedImageBackground.Auto,
    OutputFileFormat = GeneratedImageFileFormat.Png,
});

await Task.Factory.StartNew(() =>
{
    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
    {
        FileName = image.Value.ImageUri.AbsoluteUri, //WARNING: IMAGES ARE 'PUBLIC'!
        UseShellExecute = true
    });
});