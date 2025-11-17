using Toolkit.Comparison;

Console.Clear();

/*
- Goal 1: Make a Reasoning Agent that
  - set reasoning to low
  - output the reasoning summary
  - use structured output

- Goal 2: Make a Reasoning Agent that
  - set reasoning to low
  - output the reasoning summary
  - add function calling middleware
  - track the raw call
  - use structured output
*/

//await WithoutToolkit.Run();
await WithToolkit.Run();