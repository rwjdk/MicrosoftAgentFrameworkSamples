//YouTube video that cover this sample: https://youtu.be/vy3o-XEBzY8

using Azure;
using Azure.AI.Agents.Persistent;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Shared;

#pragma warning disable OPENAI001

Console.Clear();

Configuration configuration = ConfigurationManager.GetConfiguration();
PersistentAgentsClient client = new(configuration.AzureAiFoundryAgentEndpoint, new AzureCliCredential());

Response<PersistentAgent>? aiFoundryAgent = null;
ChatClientAgentThread? chatClientAgentThread = null;
try
{
    aiFoundryAgent = await client.Administration.CreateAgentAsync(
        configuration.ChatDeploymentName,
        "CodeGraphAgent",
        "",
        "You are a Graph-expert on US States",
        new List<ToolDefinition>
        {
            new CodeInterpreterToolDefinition()
        });

    AIAgent agent = await client.GetAIAgentAsync(aiFoundryAgent.Value.Id);

    AgentThread thread = agent.GetNewThread();

    AgentRunResponse response = await agent.RunAsync("Make a png image with graph listing population of the top 10 US States in year 2000", thread);
    Console.WriteLine(response);

    string? fileId = null;
    foreach (ChatMessage message in response.Messages)
    {
        foreach (AIContent content in message.Contents)
        {
            foreach (AIAnnotation annotation in content.Annotations ?? [])
            {
                if (annotation is CitationAnnotation citationAnnotation)
                {
                    Console.WriteLine("The intended way to get file, but not working consistently at the moment due to a bug in the OpenAI SDK");
                    fileId = citationAnnotation.FileId;
                }
            }
        }
    }

    if (fileId == null)
    {
        //The Workaround
        string threadId = ((ChatClientAgentThread)thread).ConversationId!;
        string runId = response.ResponseId!;
        await foreach (PersistentThreadMessage persistentThreadMessage in client.Messages.GetMessagesAsync(threadId, runId))
        {
            foreach (MessageContent contentItem in persistentThreadMessage.ContentItems)
            {
                if (contentItem is MessageImageFileContent messageImageFileContent)
                {
                    fileId = messageImageFileContent.FileId;
                }
                else if (contentItem is MessageTextContent messageTextContent)
                {
                    foreach (MessageTextAnnotation annotation in messageTextContent.Annotations)
                    {
                        if (annotation is MessageTextFilePathAnnotation messageTextFilePathAnnotation)
                        {
                            fileId = messageTextFilePathAnnotation.FileId;
                        }

                        if (annotation is MessageTextFileCitationAnnotation messageTextFileCitationAnnotation)
                        {
                            fileId = messageTextFileCitationAnnotation.FileId;
                        }
                    }
                }
            }
        }
    }

    if (fileId != null)
    {
        Response<BinaryData> fileContent = await client.Files.GetFileContentAsync(fileId);
        string path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".png");
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
finally
{
    if (chatClientAgentThread != null)
    {
        await client.Threads.DeleteThreadAsync(chatClientAgentThread.ConversationId);
    }

    if (aiFoundryAgent != null)
    {
        await client.Administration.DeleteAgentAsync(aiFoundryAgent.Value.Id);
    }
}