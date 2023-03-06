using System.Diagnostics;
using System.Text;

namespace Knapcode.TorSharp.Tools
{
    internal class WhereisUtility
    {
        public static string Whereis(string searchPattern)
        {
            using (var process = new Process())
            {
                process.StartInfo.FileName = "whereis";
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
