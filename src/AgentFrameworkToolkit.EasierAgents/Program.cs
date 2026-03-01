using AgentFrameworkToolkit.EasierAgents;
using Shared;

Console.Clear();

Utils.Green("Before");
await Before.RunAsync();

Utils.Separator();

Utils.Green("After");
await After.RunAsync();