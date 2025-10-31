//WARNING: This is a playground area for the creator of the Repo to test and tinker. Nothing in this project is as such educational and might not even execute properly

//Notes
//- Microsoft.Agents.AI.Hosting.AgentCatalog TODO: Guess this is something to be used in AI Foundry

#pragma warning disable OPENAI001
using A2A;
using Azure;
using Azure.AI.Agents.Persistent;
using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.Extensions.AI;
using OpenAI;
using OpenAI.Images;
using Playground.Tests;
using Shared;
using Shared.Extensions;
using System.ClientModel;
using System.Collections;
using System.Reflection;
using System.Text;
using System.Threading;
using ImageGenerationOptions = OpenAI.Images.ImageGenerationOptions;

Console.WriteLine("");
Console.Clear();

Configuration configuration = ConfigurationManager.GetConfiguration();

//AzureOpenAIClient client = new(new Uri(configuration.AzureOpenAiEndpoint), new ApiKeyCredential(configuration.AzureOpenAiKey));
OpenAIClient client = new(configuration.OpenAiApiKey);
ImageClient imageClient = client.GetImageClient("gpt-image-1");
ClientResult<GeneratedImage> image = await imageClient.GenerateImageAsync("A Tiger in a jungle with a party-hat", new ImageGenerationOptions
{
    Background = GeneratedImageBackground.Auto,
    Quality = GeneratedImageQuality.Auto,
    Size = GeneratedImageSize.W1024xH1024,
    OutputFileFormat = GeneratedImageFileFormat.Png,
});
byte[] bytes = image.Value.ImageBytes.ToArray();
string path = Path.Combine(Path.GetTempPath(), "image.png");
File.WriteAllBytes(path, bytes);

await Task.Factory.StartNew(() =>
{
    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
    {
        FileName = path,
        UseShellExecute = true
    });
});

Console.WriteLine();

//await AzureOpenAiFoundry.Run(configuration);
//await FileTool.Run(configuration);
//await CodeTool.Run(configuration);
//await ReasoningSummary.Run(configuration);
//await CodexSpecialModels.Run(configuration);
//await SpaceNewsWebSearch.Run(configuration);
//await ResumeConversation.Run(configuration);
//await AzureOpenAiCodex.Run(configuration);