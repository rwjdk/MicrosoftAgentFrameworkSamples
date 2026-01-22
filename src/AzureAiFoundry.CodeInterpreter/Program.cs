//YouTube video that cover this sample: https://youtu.be/vy3o-XEBzY8

using Azure;
using Azure.AI.Agents.Persistent;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Shared;

Console.Clear();

Secrets secrets = SecretManager.GetSecrets();
PersistentAgentsClient client = new(secrets.AzureAiFoundryAgentEndpoint, new AzureCliCredential());

Response<PersistentAgent>? aiFoundryAgent = null;
ChatClientAgentThread? chatClientAgentThread = null;
try
{
    aiFoundryAgent = await client.Administration.CreateAgentAsync(
        secrets.ChatDeploymentName,
        "CodeGraphAgent",
        "",
        "You are a Graph-expert on US States",
        new List<ToolDefinition>
        {
            new CodeInterpreterToolDefinition()
        });

    AIAgent agent = await client.GetAIAgentAsync(aiFoundryAgent.Value.Id);

    AgentThread thread = await agent.GetNewThreadAsync();

    AgentResponse response = await agent.RunAsync("Make a jpg image with graph listing population of the top 10 US States in year 2000", thread);

    string? fileId = null;
    string? filename = null;
    string? filePath = null;
    string? textToReplace = null;
    foreach (ChatMessage message in response.Messages)
    {
        foreach (AIContent content in message.Contents)
        {
            foreach (AIAnnotation annotation in content.Annotations ?? [])
            {
                if (annotation.RawRepresentation is TextAnnotationUpdate citationAnnotation)
                {
                    fileId = citationAnnotation.OutputFileId;
                    textToReplace = citationAnnotation.TextToReplace;
                    filename = Path.GetFileName(textToReplace);
                }
            }
        }
    }

    if (fileId != null)
    {
        Response<BinaryData> fileContent = await client.Files.GetFileContentAsync(fileId);
        filePath = Path.Combine(Path.GetTempPath(), filename!);
        await File.WriteAllBytesAsync(filePath, fileContent.Value.ToArray());
        await Task.Factory.StartNew(() =>
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = filePath,
                UseShellExecute = true
            });
        });
    }

    Console.WriteLine(textToReplace != null ? response.Text.Replace(textToReplace, filePath) : response.Text);
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