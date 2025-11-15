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

        AgentRunResponse response;
        switch (_selectedPersona)
        {
            case ChatPersona.ComicBookGuy:
                response = await comicBookAgent.RunAsync(_question);
                break;
            case ChatPersona.Assistant:
                response = await assistantAgent.RunAsync(_question);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        _answer = response.Text;
        StateHasChanged();
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