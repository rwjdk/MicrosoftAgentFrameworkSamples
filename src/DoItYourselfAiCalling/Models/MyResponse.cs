namespace DoItYourselfAiCalling.Models;

internal class MyResponse
{
    public required IList<MyMessage> Messages { get; set; }

    public string Text => Messages.Last().Content;

    public override string ToString()
    {
        return Text;
    }
}