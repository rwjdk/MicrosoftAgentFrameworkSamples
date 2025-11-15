using JetBrains.Annotations;
using Microsoft.Agents.AI;

namespace BlazorWasm.FrontEnd.Pages;

[UsedImplicitly]
public partial class Home([FromKeyedServices("comic-book-guy-agent")] ChatClientAgent comicBookAgent)
{
    private string? _question;
    private string? _answer;

    private async Task AskAi()
    {
        if (string.IsNullOrWhiteSpace(_question))
        {
            return;
        }

        AgentRunResponse response = await comicBookAgent.RunAsync(_question);
        _answer = response.Text;
        StateHasChanged();
    }
}