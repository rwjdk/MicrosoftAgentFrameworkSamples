using JetBrains.Annotations;
using Microsoft.Agents.AI;

namespace BlazorWasm.FrontEnd.Pages;

[UsedImplicitly]
public partial class Home([FromKeyedServices("comic-book-guy-agent")] ChatClientAgent comicBookAgent, [FromKeyedServices("assistant-agent")] ChatClientAgent assistantAgent)
{
    private string? _question;
    private string? _answer;
    private ChatPersona _selectedPersona = ChatPersona.ComicBookGuy;

    private async Task AskAi()
    {
        if (string.IsNullOrWhiteSpace(_question))
        {
            return;
        }

        AIAgent agentToUse = _selectedPersona switch
        {
            ChatPersona.ComicBookGuy => comicBookAgent,
            ChatPersona.Assistant => assistantAgent,
            _ => throw new ArgumentOutOfRangeException()
        };

        _answer = string.Empty;
        await foreach (AgentRunResponseUpdate update in agentToUse.RunStreamingAsync(_question))
        {
            _answer += update.Text;
            StateHasChanged();
        }
    }

    private void SetPersona(ChatPersona persona)
    {
        _selectedPersona = persona;
    }

    private string GetPersonaButtonClass(ChatPersona persona) =>
        persona == _selectedPersona ? "persona-button active" : "persona-button";

    private enum ChatPersona
    {
        ComicBookGuy,
        Assistant
    }
}