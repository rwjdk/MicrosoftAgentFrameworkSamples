//YouTube video that cover this sample https://youtu.be/F8BxvnpWJ9s
//NB: This samples is not as such related to Microsoft Agent Framework, but output can of cause be used

using GenerativeAI;
using GenerativeAI.Clients;
using GenerativeAI.Types;
using Shared;

Console.Clear();

Configuration configuration = ConfigurationManager.GetConfiguration();

GoogleAi googleAi = new(configuration.GoogleGeminiApiKey);

await UsingGeminiModel(googleAi);
await UsingImageModel(googleAi);

async Task UsingGeminiModel(GoogleAi googleAi1)
{
    GeminiModel geminiModel = googleAi1.CreateGeminiModel("gemini-2.5-flash-image"); //aka NanoBanana

    GenerateContentResponse generateContentResponse = await geminiModel.GenerateContentAsync("Image of a Tiger in a jungle with a party-hat");

    if (generateContentResponse.Candidates![0].Content is Content content)
    {
        foreach (Part part in content.Parts)
        {
            if (part.InlineData != null)
            {
                byte[] imageBytes = Convert.FromBase64String(part.InlineData.Data!);
                string path = Path.Combine(Path.GetTempPath(), $"image-{Guid.NewGuid():N}.png");
                File.WriteAllBytes(path, imageBytes);
                await ShowImage(path);
            }
        }
    }
}

async Task UsingImageModel(GoogleAi googleAi2)
{
    ImagenModel imageModel = googleAi2.CreateImageModel("imagen-4.0-generate-001");

    GenerateImageResponse response = (await imageModel.GenerateImagesAsync("A Tiger in a jungle with a party-hat", parameters:
        new ImageGenerationParameters
        {
            SampleCount = 1,
        }))!;

    foreach (VisionGenerativeModelResult result in response.Predictions!)
    {
        byte[] imageBytes = Convert.FromBase64String(result.BytesBase64Encoded!);
        string path = Path.Combine(Path.GetTempPath(), $"image-{Guid.NewGuid():N}.png");
        File.WriteAllBytes(path, imageBytes);
        await ShowImage(path);
    }
}

async Task ShowImage(string path)
{
    await Task.Factory.StartNew(() =>
    {
        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
        {
            FileName = path,
            UseShellExecute = true
        });
    });
}