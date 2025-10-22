using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI;
using OpenAI.Containers;
using OpenAI.Responses;
using Shared;
using Shared.Extensions;
using System.ClientModel;

namespace Playground.Tests;

#pragma warning disable OPENAI001
public class CodeTool
{
    public static async Task Run(Configuration configuration)
    {
        OpenAIClient client = new(configuration.OpenAiApiKey);
        //NB: I was unable to get this to work with Azure OpenAI in regard to downloading files from Code Interpreter
        AIAgent agent = client
            .GetOpenAIResponseClient("gpt-4.1")
            .CreateAIAgent(tools: [new HostedCodeInterpreterTool()]);

        AgentRunResponse response = await agent.RunAsync("Find Top 10 Countries in the world and make a Bar chart should each countries population in millions");
        foreach (var message in response.Messages)
        {
            foreach (AIContent content in message.Contents)
            {
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
    }
}