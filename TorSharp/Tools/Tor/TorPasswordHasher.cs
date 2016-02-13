using System;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Knapcode.TorSharp.Tools.Tor
{
    public class TorPasswordHasher
    {
        public string HashPassword(Tool tor, string password)
        {
            var process = new Process
            {
                StartInfo =
                {
                    FileName = tor.ExecutablePath,
                    Arguments = $"--hash-password {password}",
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            var stdout = new StringBuilder();
            var stderr = new StringBuilder();

            process.OutputDataReceived += (sender, args) => stdout.AppendLine(args.Data);
            process.ErrorDataReceived += (sender, args) => stderr.AppendLine(args.Data);

            process.Start();
            process.BeginOutputReadLine();
            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                throw new TorSharpException($"Tor had exit code {process.ExitCode} (not 0) when hashing the password.");
            }

            return stdout
                .ToString()
                .Split('\n')
                .Select(l => l.Trim())
                .Last(l => l.Length > 0);
        }
    }
}
