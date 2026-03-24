using Google.GenAI;
using Google.GenAI.Types;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Shared;
using Shared.Extensions;

Utils.Init("Google Gemini (WebSearch)");
Secrets secrets = SecretsManager.GetSecrets();
Client client = new(apiKey: secrets.GoogleGeminiApiKey);
IChatClient iChatClient = client.AsIChatClient("gemini-3-flash-preview");

string question = "What is today's Space news? (Show today's date + Answer in max 20 words + a link)";

Utils.Green("No Web Search Tool");
ChatClientAgent normalAgent = new(iChatClient);
AgentResponse response1 = await normalAgent.RunAsync(question);
Console.WriteLine(response1);
response1.Usage.OutputAsInformation();

Utils.Separator();

Utils.Green("Web Search Tool (Easy)");
ChatClientAgent webSearchAgent = new(iChatClient, tools: [new HostedWebSearchTool()]);
AgentResponse response2 = await webSearchAgent.RunAsync(question);
Console.WriteLine(response2);
response2.Usage.OutputAsInformation();

Utils.Separator();

Utils.Green("Web Search Tool (Advanced)");
ChatClientAgent webSearchAdvancedAgent = new(
    iChatClient,
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
                        GoogleSearch = new GoogleSearch
                        {
                            SearchTypes = new SearchTypes
                            {
                                WebSearch = new WebSearch()
                            }
                        }
                    }
                ]
            }
        }
    });

AgentResponse response3 = await webSearchAdvancedAgent.RunAsync(question);
Console.WriteLine(response3);
response3.Usage.OutputAsInformation();

//Web Search Cost: 5000 prompts per month (free), then $14 / 1000 search queries
