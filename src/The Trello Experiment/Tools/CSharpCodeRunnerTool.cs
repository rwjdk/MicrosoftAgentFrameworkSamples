using System.Text.Json;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Shared;

namespace The_Trello_Experiment.Tools;

public static class CSharpCodeRunnerTool
{
    public static async Task<string> ExecuteAndReturnAsync(string cSharpCode)
    {
        Utils.Gray("About to execute Code\n" + cSharpCode + "... \n\nApprove (Y/N)\n");
        ConsoleKeyInfo key = Console.ReadKey();
        if (key.Key == ConsoleKey.Y)
        {
            Console.WriteLine();
            Utils.Green("Executing Code");
            try
            {
                ScriptOptions options = ScriptOptions.Default
                    .AddReferences(
                        typeof(object).Assembly,
                        typeof(HttpClient).Assembly,
                        typeof(List<>).Assembly,
                        typeof(JsonSerializer).Assembly
                    )
                    .AddImports(
                        "System",
                        "System.Net.Http",
                        "System.Threading.Tasks",
                        "System.Collections.Generic",
                        "System.Linq",
                        "System.Text.Json.Serialization"
                    );

                // EvaluateAsync<string> expects the script to result in a string value
                string returnValue = await CSharpScript.EvaluateAsync<string>(cSharpCode, options);
                Utils.Gray("Data Back from Code\n" + returnValue);
                return returnValue;
            }
            catch (CompilationErrorException e)
            {
                string error = $"Compile Error: {string.Join(", ", e.Diagnostics)}";
                Utils.Red(error);
                return error;
            }
            catch (Exception e)
            {
                string error = $"Runtime Error: {e.Message}";
                Utils.Red(error);
                return error;
            }
        }
        else
        {
            Utils.Yellow("Abort code execution");
            return "No code was run";
        }
    }
}