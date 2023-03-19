using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace Knapcode.TorSharp.Tools
{
    internal static class WhichUtility
    {
        public static string Which(TorSharpSettings settings, string searchPattern)
        {
            if (settings.OSPlatform == TorSharpOSPlatform.Windows)
                return SearchInPathVariable(searchPattern);

            using (var process = new Process())
            {
                process.StartInfo.FileName = "which";
                process.StartInfo.Arguments = searchPattern;
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

                return output.ToString();
            }
        }

        private static string SearchInPathVariable(string pattern)
        {
            var variables = Environment.GetEnvironmentVariables();
            if (!variables.Contains("Path"))
                return null;

            var what = variables["Path"].ToString()
                .Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                .SelectMany(a => Directory.GetFiles(a, pattern, SearchOption.TopDirectoryOnly));

            return what.FirstOrDefault();
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
