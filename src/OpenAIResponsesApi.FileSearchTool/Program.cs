//YouTube video that cover this sample: https://youtu.be/jq_gIydI8m4

using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI;
using OpenAI.Files;
using OpenAI.VectorStores;
using Shared;
using Shared.Extensions;
using System.ClientModel;

#pragma warning disable OPENAI001
Console.Clear();
Configuration configuration = ConfigurationManager.GetConfiguration();
OpenAIClient client = new(configuration.OpenAiApiKey);
//NB: I was unable to get this to work with Azure OpenAI in regard to uploading files

OpenAIFileClient fileClient = client.GetOpenAIFileClient();
VectorStoreClient vectorStoreClient = client.GetVectorStoreClient();

string? fileId = null;
string? vectorStoreId = null;
try
{
    string filename = "secretData.pdf";
    byte[] fileBytes = await File.ReadAllBytesAsync(Path.Combine("Data", filename));
    ClientResult<OpenAIFile> uploadedFile = await fileClient.UploadFileAsync(new BinaryData(fileBytes), filename, FileUploadPurpose.UserData);
    fileId = uploadedFile.Value.Id;

    ClientResult<VectorStore> vectorStore = await vectorStoreClient.CreateVectorStoreAsync(options: new VectorStoreCreationOptions
    {
        Name = "MyVectorStore"
    });
    vectorStoreId = vectorStore.Value.Id;

    await vectorStoreClient.AddFileToVectorStoreAsync(vectorStore.Value.Id, uploadedFile.Value.Id);


    //NB: I was unable to get this to work with Azure OpenAI in regard to downloading files from Code Interpreter
    AIAgent agent = client
        .GetOpenAIResponseClient("gpt-4.1")
        .CreateAIAgent(
            instructions: "Only use tools. Never your world-knowledge",
            tools:
            [
                new HostedFileSearchTool
                {
                    Inputs = [new HostedFileContent(uploadedFile.Value.Id), new HostedVectorStoreContent(vectorStore.Value.Id)]
                }
            ]);

    AgentRunResponse response = await agent.RunAsync("What is word of the day?");
    Console.Write(response);
    response.Usage.OutputAsInformation();
}
finally
{
    if (vectorStoreId != null)
    {
        await vectorStoreClient.DeleteVectorStoreAsync(vectorStoreId);
    }

    if (fileId != null)
    {
        await fileClient.DeleteFileAsync(fileId);
    }
}