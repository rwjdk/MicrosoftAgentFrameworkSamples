using System.Diagnostics;
using System.Text;

namespace AgentSkills;

public static class PythonRunner
{
    public static string RunPhytonScript(string pythonFilePath)
    {
        if (string.IsNullOrWhiteSpace(pythonFilePath))
        {
            throw new ArgumentException("File path cannot be empty.", nameof(pythonFilePath));
        }

        if (!File.Exists(pythonFilePath))
        {
            throw new FileNotFoundException("Python file not found.", pythonFilePath);
        }

        if (!string.Equals(Path.GetExtension(pythonFilePath), ".py", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("File must have a .py extension.", nameof(pythonFilePath));
        }

        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = "python",
            Arguments = "\"" + pythonFilePath + "\"",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8
        };

        using (Process process = new Process { StartInfo = startInfo })
        {
            process.Start();

            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();

            process.WaitForExit();

            if (process.ExitCode != 0 || !string.IsNullOrWhiteSpace(error))
            {
                throw new InvalidOperationException(
                    "Python script failed. ExitCode=" + process.ExitCode + Environment.NewLine + error.Trim());
            }

            return output.TrimEnd();
        }
    }
}