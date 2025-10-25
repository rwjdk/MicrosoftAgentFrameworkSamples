//YouTube video that cover this sample: https://youtu.be/Eh1D3VD-708

using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI;
using OpenAI.Containers;
using Shared;
using Shared.Extensions;
using System.ClientModel;
using Utils = Shared.Utils;

#pragma warning disable OPENAI001
Configuration configuration = ConfigurationManager.GetConfiguration();

OpenAIClient client = new(configuration.OpenAiApiKey);
//NB: I was unable to get this to work with Azure OpenAI in regard to downloading files from Code Interpreter
AIAgent agent = client
    .GetOpenAIResponseClient("gpt-4.1")
    .CreateAIAgent(tools: [new HostedCodeInterpreterTool()]);

string question = "Find Top 10 Countries in the world and make a Bar chart should each countries population in millions";
AgentRunResponse response = await agent.RunAsync(question);
foreach (var message in response.Messages)
{
    foreach (AIContent content in message.Contents)
    {
        foreach (AIAnnotation annotation in content.Annotations ?? [])
        {
            if (annotation is CitationAnnotation citationAnnotation)
            {
                Console.WriteLine("The intended way to get file, but not working at the moment due to a bug in the OpenAI SDK");
            }
        }

        //This is the workaround
        if (content.RawRepresentation is OpenAI.Responses.CodeInterpreterCallResponseItem codeInterpreterCallResponse)
        {
            Utils.WriteLineGreen("The Code");
            Utils.WriteLineDarkGray(codeInterpreterCallResponse.Code);

            Utils.WriteLineGreen("The File");
            ContainerClient containerClient = client.GetContainerClient();
            string containerId = codeInterpreterCallResponse.ContainerId;
            CollectionResult<ContainerFileResource> containerFileResources = containerClient.GetContainerFiles(containerId);
            foreach (ContainerFileResource fileResource in containerFileResources)
            {
                ClientResult<BinaryData> fileContent = await containerClient.GetContainerFileContentAsync(containerId, fileResource.Id);
                string path = Path.Combine(Path.GetTempPath(), fileResource.Path.Replace("/", "_"));
                await File.WriteAllBytesAsync(path, fileContent.Value.ToArray());
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
}

Console.Write(response);

response.Usage.OutputAsInformation();