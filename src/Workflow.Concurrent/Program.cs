//YouTube video that cover this sample: https://youtu.be/qYxGJ-D3Tl0

using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using OpenAI;
using OpenAI.Chat;
using Shared;
using System.ClientModel;
using ChatMessage = Microsoft.Extensions.AI.ChatMessage;

Configuration configuration = ConfigurationManager.GetConfiguration();

AzureOpenAIClient client = new(new Uri(configuration.AzureOpenAiEndpoint), new ApiKeyCredential(configuration.AzureOpenAiKey));

ChatClient chatClient = client.GetChatClient("gpt-4.1");

ChatClientAgent legalAgent = chatClient.CreateAIAgent(name: "LegalAgent", instructions: "You are a legal agent that need to evaluate if a text is legal (use max 200 chars)");
ChatClientAgent spellingErrorAgent = chatClient.CreateAIAgent(name: "SpellingErrorAgent", instructions: "You are a spelling expert (use max 200 chars)");

Workflow workflow = AgentWorkflowBuilder.BuildConcurrent([legalAgent, spellingErrorAgent]);


// ReSharper disable once StringLiteralTypo
string legalText = """
                   This Legal Disclaimer (“Agreement”) governs the ownership, maintenance, and care of domesticated ducks 
                   kept as personal pets. By acquiring or housing a duck, the Owner hereby acknowledges and agrees to 
                   comply with all applicable municipal and federal regulations concerning the keeping of live poultry. 
                   The Owner affirms responsibility for providing humane living conditions, including adequate shelter, 
                   food, and access to clean water. Ducks must not be subjected to neglect, cruelty, or abandonment.
                   The Owner shall maintain sanitary standards to prevent odors, noise disturbance, or the spread of 
                   disease to neighboring properties. Local authorities reserve the right to inspect premises upon 
                   reasonable notice to ensure compliance. Any sale or transfer of pet ducks must include written 
                   documentation verifying the animal’s health status and vaccination records where required.
                   This Agreement does not confer any breeding or commercial rights unless expressly authorized in 
                   writing by the relevant agency. The Owner indemnifies and holds harmless all regulatory bodies 
                   against claims arising from damage or injury caused by said animals. Failure to adhere to the 
                   provisions herein may result in fines, forfeiture, or legal action.
                   Acceptance of a duck as a pet constitutes full consent to these terms and any subsequent 
                   amendmants or revisions adopted by the governing authority.
                   """;

var messages = new List<ChatMessage> { new(ChatRole.User, legalText) };

StreamingRun run = await InProcessExecution.StreamAsync(workflow, messages);
await run.TrySendMessageAsync(new TurnToken(emitEvents: true));

List<ChatMessage> result = [];
await foreach (WorkflowEvent evt in run.WatchStreamAsync().ConfigureAwait(false))
{
    if (evt is WorkflowOutputEvent completed)
    {
        result = (List<ChatMessage>)completed.Data!;
        break;
    }
}

foreach (var message in result.Where(x => x.Role != ChatRole.User))
{
    Utils.WriteLineGreen(message.AuthorName ?? "Unknown");
    Console.WriteLine($"{message.Text}");
    Utils.Separator();
}