using Google.GenAI;
using Google.GenAI.Types;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Shared;
using Shared.Extensions;
using Environment = System.Environment;

Utils.Init("Google Gemini (Google Maps)");
Secrets secrets = SecretsManager.GetSecrets();
Client client = new(apiKey: secrets.GoogleGeminiApiKey);
IChatClient iChatClient = client.AsIChatClient("gemini-3-flash-preview");

string question = "What is the opening times of Hard Rock cafe in New York. And tell me if they have wheelchair access";

ChatClientAgent agent = new(iChatClient,
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
                        GoogleMaps = new GoogleMaps
                        {
                            EnableWidget = true
                        }
                    }
                ]
            }
        }
    });

AgentResponse response = await agent.RunAsync(question);

Console.WriteLine(response);
response.Usage.OutputAsInformation();

//More detailed data
if (response.RawRepresentation is ChatResponse { RawRepresentation: GenerateContentResponse generateContentResponse })
{
    foreach (Candidate candidate in generateContentResponse.Candidates ?? [])
    {
        GroundingMetadata? groundingMetadata = candidate.GroundingMetadata;
        if (groundingMetadata != null)
        {
            //Widget can be displayed using Google Maps Javascript API
            //https://developers.google.com/maps/documentation/javascript/load-maps-js-api
            string? widget = groundingMetadata.GoogleMapsWidgetContextToken;

            //Grounding data
            foreach (GroundingChunk chunk in groundingMetadata.GroundingChunks ?? [])
            {
                if (chunk.Maps != null)
                {
                    Utils.Yellow("- URL: " + chunk.Maps.Uri);
                    Utils.Yellow("- Title: " + chunk.Maps.Title);
                    Utils.Yellow("- Text: " + Environment.NewLine + chunk.Maps.Text);
                }
            }

        }
    }
}

//Maps Cost: 5000 prompts per month (free), then $14 / 1000 queries