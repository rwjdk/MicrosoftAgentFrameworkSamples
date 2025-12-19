//YouTube video that cover this sample: https://youtu.be/AJZhHHnsFXY
// ReSharper disable UnreachableSwitchCaseDueToIntegerAnalysis

using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI;
using OpenAI.Chat;
using Shared;
using Shared.Extensions;
using System.ClientModel;
using ChatMessage = Microsoft.Extensions.AI.ChatMessage;

Secrets secrets = SecretManager.GetSecrets();

AzureOpenAIClient azureOpenAiClient = new(new Uri(secrets.AzureOpenAiEndpoint), new ApiKeyCredential(secrets.AzureOpenAiKey));
OpenAIClient openAiClient = new(new ApiKeyCredential(secrets.OpenAiApiKey));

ChatClientAgent azureOpenAiAgent = azureOpenAiClient.GetChatClient(secrets.ChatDeploymentName).CreateAIAgent();
ChatClientAgent openAiAgent = openAiClient.GetChatClient(secrets.ChatDeploymentName).CreateAIAgent();

Scenario scenario = Scenario.Image;

AgentRunResponse response;
switch (scenario)
{
    case Scenario.Text:
    {
        response = await azureOpenAiAgent.RunAsync(new ChatMessage(ChatRole.User, "What is the capital of France?"));
        ShowResponse(response);
    }
        break;

    case Scenario.Image:
    {
        //---------------------------------------------------------------------------------
        //Image via URI
        response = await azureOpenAiAgent.RunAsync(new ChatMessage(ChatRole.User,
        [
            new TextContent("What is in this image?"),
            new UriContent("https://upload.wikimedia.org/wikipedia/commons/7/70/A_game_of_Settlers_of_Catan.jpg", "image/jpeg")
        ]));
        ShowResponse(response);
        //---------------------------------------------------------------------------------
        //Local File
        string path = Path.Combine("SampleData", "image.jpg");

        //Image via Base64
        string base64Pdf = Convert.ToBase64String(File.ReadAllBytes(path));
        string dataUri = $"data:image/jpeg;base64,{base64Pdf}";
        response = await azureOpenAiAgent.RunAsync(new ChatMessage(ChatRole.User,
        [
            new TextContent("What is in this image?"),
            new DataContent(dataUri, "image/jpeg")
        ]));
        ShowResponse(response);
        //---------------------------------------------------------------------------------
        //Image via Memory
        ReadOnlyMemory<byte> data = File.ReadAllBytes(path).AsMemory();
        response = await azureOpenAiAgent.RunAsync(new ChatMessage(ChatRole.User,
        [
            new TextContent("What is in this image?"),
            new DataContent(data, "image/jpeg")
        ]));
        ShowResponse(response);
        //---------------------------------------------------------------------------------
    }
        break;
    case Scenario.Pdf:
    {
        //Notes
        //- This Scenario only work on OpenAI. Not AzureOpenAI!
        //- PDFs can't be read via URI; Only 'local' data
        //---------------------------------------------------------------------------------
        string path = Path.Combine("SampleData", "catan_rules.pdf");
        //---------------------------------------------------------------------------------
        //PDF as Base64
        string base64Pdf = Convert.ToBase64String(File.ReadAllBytes(path));
        string dataUri = $"data:application/pdf;base64,{base64Pdf}";
        response = await openAiAgent.RunAsync(new ChatMessage(ChatRole.User,
        [
            new TextContent("What is the winning condition in attached PDF"),
            new DataContent(dataUri, "application/pdf")
        ]));
        ShowResponse(response);
        //---------------------------------------------------------------------------------
        //PDF as Memory
        ReadOnlyMemory<byte> data = File.ReadAllBytes(path).AsMemory();
        response = await openAiAgent.RunAsync(new ChatMessage(ChatRole.User,
        [
            new TextContent("What is the winning condition in attached PDF"),
            new DataContent(data, "application/pdf"),
        ]));
        ShowResponse(response);
        //---------------------------------------------------------------------------------
    }
        break;
    default:
        throw new ArgumentOutOfRangeException();
}


void ShowResponse(AgentRunResponse agentRunResponse)
{
    Console.WriteLine(agentRunResponse);
    agentRunResponse.Usage.OutputAsInformation();
}

enum Scenario
{
    Text,
    Pdf,
    Image,
}