using Azure.AI.OpenAI;
using Shared;
using System.ClientModel;
using The_Trello_Experiment;

Console.Clear();
Secrets secrets = SecretManager.GetSecrets();

AzureOpenAIClient azureOpenAIClient = new AzureOpenAIClient(
    new Uri(secrets.AzureOpenAiEndpoint),
    new ApiKeyCredential(secrets.AzureOpenAiKey));

//await Take1OpenAICodeInterpreter.Run(azureOpenAIClient, secrets.TrelloApiKey, secrets.TrelloToken);
//await Take2CSharpCodeRunner.Run(azureOpenAIClient, secrets.TrelloApiKey, secrets.TrelloToken);
await Take3TheSensibleChoice.Run(azureOpenAIClient, secrets.TrelloApiKey, secrets.TrelloToken);