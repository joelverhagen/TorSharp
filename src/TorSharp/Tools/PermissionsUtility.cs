using System;
using System.Diagnostics;
using System.Text;

namespace Knapcode.TorSharp.Tools
{
    internal static class PermissionsUtility
    {
        public static void MakeExecutable(TorSharpSettings settings, string path)
        {
            if (settings.OSPlatform == TorSharpOSPlatform.Windows)
            {
                return;
            }
            else if (settings.OSPlatform == TorSharpOSPlatform.Linux)
            {
                // We shell out here since invoking "stat" is non-trivial. We should invoke "chmod" but we would need
                // to know the initial permissions to perform an additive "+x" change which "stat" could tell us. Let's
                // just keep things simple.
                using (var process = new Process())
                {
                    process.StartInfo.FileName = "chmod";
                    process.StartInfo.Arguments = $"+x \"{path}\"";
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.RedirectStandardError = true;
                    process.StartInfo.CreateNoWindow = true;
                    process.StartInfo.UseShellExecute = false;

                    var output = new StringBuilder();
                    var outputLock = new object();
                    process.OutputDataReceived += GetOutputHandler(output, outputLock);
                    process.ErrorDataReceived += GetOutputHandler(output, outputLock);

                    process.Start();
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();
                    process.WaitForExit();

                    if (process.ExitCode != 0)
                    {
                        throw new TorSharpException(
                            $"Failed making file '{path}' executable with 'chmod'. Exit code: {process.ExitCode}. " +
                            $"Output: {Environment.NewLine}{output}");
                    }
                }
            }
            else
            {
                settings.RejectRuntime("make a file executable");
            }
        }

        private static DataReceivedEventHandler GetOutputHandler(StringBuilder output, object outputLock)
        {
            return (object sender, DataReceivedEventArgs e) =>
            {
                lock (outputLock)
                {
                    output.AppendLine(e.Data);
                }
            };
        }
    }
}
