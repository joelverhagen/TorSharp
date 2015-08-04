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
                    RedirectStandardOutput = true,
                    UseShellExecute = false
                }
            };

            var sb = new StringBuilder();
            process.OutputDataReceived += (sender, args) => sb.AppendLine(args.Data);

            process.Start();
            process.BeginOutputReadLine();
            process.WaitForExit();

            return sb
                .ToString()
                .Split('\n')
                .Select(l => l.Trim())
                .Last(l => l.Length > 0);
        }
    }
}
