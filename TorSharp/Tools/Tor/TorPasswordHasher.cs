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
                    FileName = "cmd.exe",
                    Arguments = $"/c \"{tor.ExecutablePath} --hash-password {password} | more\"",
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false
                }
            };

            var stdout = new StringBuilder();
            var stderr = new StringBuilder();

            process.OutputDataReceived += (sender, args) => stdout.AppendLine(args.Data);
            process.ErrorDataReceived += (sender, args) => stderr.AppendLine(args.Data);

            process.Start();
            process.BeginOutputReadLine();
            process.WaitForExit();

            Console.WriteLine("Path: " + tor.ExecutablePath);
            Console.WriteLine("Arguments : " + process.StartInfo.Arguments);
            Console.WriteLine("stdout:" + stdout);
            Console.WriteLine("stderr:" + stderr);
            Console.WriteLine("Exit code: " + process.ExitCode);

            return stdout
                .ToString()
                .Split('\n')
                .Select(l => l.Trim())
                .Last(l => l.Length > 0);
        }
    }
}
