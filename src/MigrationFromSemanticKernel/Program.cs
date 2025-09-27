using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using Shared;

Configuration configuration = ConfigurationManager.GetConfiguration();
IKernelBuilder kernelBuilder = Kernel.CreateBuilder();
kernelBuilder.AddAzureOpenAIChatCompletion(configuration.ChatDeploymentName, configuration.Endpoint, configuration.Key);
Kernel kernel = kernelBuilder.Build();

ChatCompletionAgent agent = new()
{
    Kernel = kernel,
    Instructions = "You are a friendly AI, helping the user to answer questions",
};

List<ChatMessageContent> conversation = [];
while (true)
{
    Console.Write("> ");
    string? inputFromUser = Console.ReadLine();
    if (!string.IsNullOrWhiteSpace(inputFromUser))
    {
        conversation.Add(new ChatMessageContent(AuthorRole.User, inputFromUser));
        string output = string.Empty;
        await foreach (AgentResponseItem<StreamingChatMessageContent> response in agent.InvokeStreamingAsync(conversation))
        {
            output += response.Message;
            Console.Write(response.Message);
        }

        conversation.Add(new ChatMessageContent(AuthorRole.Assistant, output));
    }

    Console.WriteLine();
    Console.WriteLine(string.Empty.PadLeft(50, '*'));
    Console.WriteLine();
}