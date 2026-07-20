
namespace DoItYourselfAiCalling.Models
{
    record Provider(string Endpoint, string ApiKey, string Model)
    {
        public MyAgent AsAgent(string? instructions, IList<MyTool>? tools = null)
        {
            return new MyAgent
            {
                Provider = this,
                Instructions = instructions,
                Tools = tools ?? []
            };
        }
    }
}