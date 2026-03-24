using Google.GenAI;
using Google.GenAI.Types;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Shared;
using Shared.Extensions;
using File = System.IO.File;

Utils.Init("Google Gemini (Code Execution)");
Secrets secrets = SecretsManager.GetSecrets();
Client client = new(apiKey: secrets.GoogleGeminiApiKey);
IChatClient iChatClient = client.AsIChatClient("gemini-3-flash-preview");

string question = "Make a chart (use CodeExecution tool) of Top 5 countries by the amount cars produced per year";

ChatClientAgent agent = new(iChatClient,
    new ChatClientAgentOptions
    {
        ChatOptions = new ChatOptions
        {
            RawRepresentationFactory = _ => new GenerateContentConfig
            {
                Tools =
                [
                    new Tool
                    {
                        CodeExecution = new ToolCodeExecution()
                    }
                ]
            }
        }
    });

AgentResponse response = await agent.RunAsync(question);

Console.WriteLine(response);
response.Usage.OutputAsInformation();

//More detailed data
if (response.RawRepresentation is ChatResponse { RawRepresentation: GenerateContentResponse generateContentResponse })
{
    Utils.Yellow("Executable Code");
    Utils.Gray(generateContentResponse.ExecutableCode ?? "");

    foreach (Part part in generateContentResponse.Parts ?? [])
    {
        if (part.InlineData != null)
        {
            string path = Path.Combine(Path.GetTempPath(), "image.png");
            await File.WriteAllBytesAsync(path, part.InlineData.Data!);
            await Task.Factory.StartNew(() =>
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = path,
                    UseShellExecute = true
                });
            });
        }
    }
}


AgentResponse response2 = await agent.RunAsync("What is the sum of the first 50 prime numbers? Generate and run code for the calculation, and make sure you get all 50");
Console.WriteLine(response2);
response2.Usage.OutputAsInformation();

if (response2.RawRepresentation is ChatResponse { RawRepresentation: GenerateContentResponse generateContentResponse2 })
{
    Utils.Yellow("Executable Code");
    Utils.Gray(generateContentResponse2.ExecutableCode ?? "");

    Utils.Yellow("Code Result");
    Utils.Gray(generateContentResponse2.CodeExecutionResult ?? "");

}


//Code Execution is paid via tokens.