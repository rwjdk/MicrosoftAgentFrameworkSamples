using JetBrains.Annotations;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using OpenAI.Chat;

namespace MicrosoftAgentFramework.Utilities.Extensions;

[PublicAPI]
public static class ChatClientExtensions
{
    /// <summary>
    /// Creates an AI agent from an <see cref="ChatClient"/> using the OpenAI Chat Completion API.
    /// </summary>
    /// <param name="client">The OpenAI <see cref="ChatClient"/> to use for the agent.</param>
    /// <param name="instructions">Optional system instructions that define the agent's behavior and personality.</param>
    /// <param name="name">Optional name for the agent for identification purposes.</param>
    /// <param name="description">Optional description of the agent's capabilities and purpose.</param>
    /// <param name="tools">Optional collection of AI tools that the agent can use during conversations.</param>
    /// <param name="reasoningEffort">The reasoning Effort (if a Reasoning Model is used) Valid Values are 'minimal', 'low', 'medium' and 'high'</param>
    /// <param name="clientFactory">Provides a way to customize the creation of the underlying <see cref="IChatClient"/> used by the agent.</param>
    /// <param name="loggerFactory">Optional logger factory for enabling logging within the agent.</param>
    /// <param name="services">An optional <see cref="IServiceProvider"/> to use for resolving services required by the <see cref="AIFunction"/> instances being invoked.</param>
    /// <returns>An <see cref="ChatClientAgent"/> instance backed by the OpenAI Chat Completion service.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="client"/> is <see langword="null"/>.</exception>
    // ReSharper disable once InconsistentNaming
    public static ChatClientAgent CreateAIAgentForOpenAi(
        this ChatClient client,
        string? instructions = null,
        string? name = null,
        string? description = null,
        IList<AITool>? tools = null,
        string? reasoningEffort = null,
        Func<IChatClient, IChatClient>? clientFactory = null,
        ILoggerFactory? loggerFactory = null,
        IServiceProvider? services = null)
        => client.CreateAIAgentForAzureOpenAi(instructions, name, description, tools, reasoningEffort, clientFactory, loggerFactory, services);

    /// <summary>
    /// Creates an AI agent from an <see cref="ChatClient"/> using the OpenAI Chat Completion API.
    /// </summary>
    /// <param name="client">The OpenAI <see cref="ChatClient"/> to use for the agent.</param>
    /// <param name="instructions">Optional system instructions that define the agent's behavior and personality.</param>
    /// <param name="name">Optional name for the agent for identification purposes.</param>
    /// <param name="description">Optional description of the agent's capabilities and purpose.</param>
    /// <param name="tools">Optional collection of AI tools that the agent can use during conversations.</param>
    /// <param name="reasoningEffort">The reasoning Effort (if a Reasoning Model is used) Valid Values are 'minimal', 'low', 'medium' and 'high'</param>
    /// <param name="clientFactory">Provides a way to customize the creation of the underlying <see cref="IChatClient"/> used by the agent.</param>
    /// <param name="loggerFactory">Optional logger factory for enabling logging within the agent.</param>
    /// <param name="services">An optional <see cref="IServiceProvider"/> to use for resolving services required by the <see cref="AIFunction"/> instances being invoked.</param>
    /// <returns>An <see cref="ChatClientAgent"/> instance backed by the OpenAI Chat Completion service.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="client"/> is <see langword="null"/>.</exception>
    // ReSharper disable once InconsistentNaming
    public static ChatClientAgent CreateAIAgentForAzureOpenAi(
        this ChatClient client,
        string? instructions = null,
        string? name = null,
        string? description = null,
        IList<AITool>? tools = null,
        string? reasoningEffort = null,
        Func<IChatClient, IChatClient>? clientFactory = null,
        ILoggerFactory? loggerFactory = null,
        IServiceProvider? services = null)
    {
        ChatOptions options = new();
        if (!string.IsNullOrWhiteSpace(reasoningEffort))
        {
            options.RawRepresentationFactory = _ => new ChatCompletionOptions()
            {
#pragma warning disable OPENAI001
                ReasoningEffortLevel = reasoningEffort,
#pragma warning restore OPENAI001
            };
        }

        options.Instructions = instructions;
        if (tools?.Count > 0)
        {
            options.Tools = tools;
        }


        IChatClient chatClient = client.AsIChatClient();

        if (clientFactory is not null)
        {
            chatClient = clientFactory(chatClient);
        }

        ChatClientAgentOptions clientAgentOptions = new()
        {
            Name = name,
            Description = description,
            Instructions = instructions,
            ChatOptions = options
        };

        return new ChatClientAgent(chatClient, clientAgentOptions, loggerFactory, services);
    }
}