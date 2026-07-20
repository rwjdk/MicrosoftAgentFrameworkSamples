using System.Text.Json;

namespace DoItYourselfAiCalling.Models;

internal class MyResponse(IList<MyMessage> messages)
{
    public IList<MyMessage> Messages { get; set; } = messages;

    public string Text => Messages.Last().Content;

    public override string ToString()
    {
        return Text;
    }
}

internal class MyResponse<T>(MyResponse raw) : MyResponse(raw.Messages)
{
    public T Result => JsonSerializer.Deserialize<T>(Text)!;
}