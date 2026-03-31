using AgentFrameworkToolkit.EasierBatching.Scenarios;
using Shared;

Utils.Init("Easier Batching");

Secrets secrets = SecretsManager.GetSecrets();

await AzureOpenAIChat.RunSampleAsync(secrets);

await AzureOpenAIChatStructuredOutput.RunSampleAsync(secrets);

await OpenAIEmbedding.RunSampleAsync(secrets);