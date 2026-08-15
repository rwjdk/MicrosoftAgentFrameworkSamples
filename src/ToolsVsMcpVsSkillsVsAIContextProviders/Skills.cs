using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using System.ClientModel;
using System.ClientModel.Primitives;
using System.Text;
using OpenAI.Responses;
using Shared;
#pragma warning disable OPENAI001

namespace ToolsVsMcpVsSkillsVsAIContextProviders;

public class Skills
{
    public async Task Run()
    {
        using CustomClientHttpHandler handler = new CustomClientHttpHandler();
        using HttpClient httpClient = new HttpClient(handler);
        
        Secrets secrets = SecretsManager.GetSecrets();

        AzureOpenAIClient client = new(new Uri(secrets.AzureOpenAiEndpoint), new ApiKeyCredential(secrets.AzureOpenAiKey),new AzureOpenAIClientOptions
        {
            //Transport = new HttpClientPipelineTransport(httpClient)
        });

        AgentSkillsProvider agentSkillsProvider = new("TestData\\AgentSkills", options: new AgentSkillsProviderOptions
        {
            //Additional settings
            DisableLoadSkillApproval = true,
            DisableReadSkillResourceApproval = true,
            DisableRunSkillScriptApproval = true
        });
        
        AIAgent agentWithSkills = client
            .GetResponsesClient()
            .AsAIAgent(new ChatClientAgentOptions
            {
                ChatOptions = new ChatOptions
                {
                    Instructions = "You are a nice AI"
                },
                AIContextProviders = [agentSkillsProvider]
            }, model: "gpt-5.6-luna")
            .AsBuilder()
            .Use(ToolCallingMiddleware)
            .Build(); ;

        AgentResponse response = await agentWithSkills.RunAsync("What are the Company values?");
        Console.WriteLine(response);
        
        async ValueTask<object?> ToolCallingMiddleware(AIAgent callingAgent, FunctionInvocationContext context, Func<FunctionInvocationContext, CancellationToken, ValueTask<object?>> next, CancellationToken cancellationToken)
        {
            StringBuilder functionCallDetails = new();
            functionCallDetails.Append($"- Tool Call: '{context.Function.Name}'");
            if (context.Arguments.Count > 0)
            {
                functionCallDetails.Append($" (Args: {string.Join(",", context.Arguments.Select(x => $"[{x.Key} = {x.Value}]"))}");
            }

            Utils.Gray(functionCallDetails.ToString());

            return await next(context, cancellationToken);
        }
    }
}