namespace AgentFramework.Toolkit.Agents.Models;

public class RawCallDetails
{
    public required string RequestUrl { get; set; }
    public required string RequestJson { get; set; }
    public required string ResponseJson { get; set; }
}