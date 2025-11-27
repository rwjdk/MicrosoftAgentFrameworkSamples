using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.ML.OnnxRuntimeGenAI;

//Requirements (From the folder you want the model in):
//- Install python
//- Install HuggingFace CLI (pip install -U "huggingface_hub[cli]")
//- Download the model (hf.exe download microsoft/Phi-4-mini-instruct-onnx --include gpu/* --local-dir .)

Console.Write("Enter FolderPath of your ONNX Model: ");
string? folderPath = Console.ReadLine();

if (folderPath != null && Directory.Exists(folderPath))
{
    OnnxRuntimeGenAIChatClient chatClient = new(folderPath);
    ChatClientAgent agent = chatClient.CreateAIAgent();

    AgentRunResponse response = await agent.RunAsync("What is the Capital of Bulgaria?");
    Console.WriteLine(response);
}