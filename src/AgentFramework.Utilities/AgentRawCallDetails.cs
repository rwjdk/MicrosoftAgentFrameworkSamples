namespace AgentFramework.Utilities;

public class AgentRawCallDetails
{
    public required string RequestUrl { get; set; }
    public required string RequestJson { get; set; }
    public required string ResponseJson { get; set; }
}