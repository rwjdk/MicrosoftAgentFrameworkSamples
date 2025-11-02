using MicrosoftAgentFramework.Toolkit.AITools;

namespace Trello.Agent.Tools;

public class SimpleTools(string x)
{
    [AiToolDetails("simple_method", "some method")]
    public string Simple()
    {
        return "Helo";
    }
}