using AgentFrameworkToolkit.EasierAgents;
using Shared;

Console.Clear();

Utils.WriteLineGreen("Before");
await Before.RunAsync();

Utils.Separator();

Utils.WriteLineGreen("After");
await After.RunAsync();