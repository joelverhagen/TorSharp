using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Knapcode.NetTor.PInvoke;

namespace Knapcode.NetTor.Tools
{
    public class HiddenWindowRunner : IToolRunner
    {
        public Task StartAsync(Tool tool)
        {
            var parameters = tool.Settings.GetArguments(tool).ToArray();
            var process = new Process
            {
                StartInfo =
                {
                    WorkingDirectory = tool.WorkingDirectory,
                    FileName = tool.ExecutablePath,
                    Arguments = string.Join(" ", parameters)
                }
            };

            return Task.Run(() =>
            {
                process.Start();

                bool started = false;
                while (!started)
                {
                    var handles = WindowsUtility.GetWindowHandles(process.Id).ToArray();
                    foreach (var handle in handles)
                    {
                        if (WindowsUtility.GetWindowText(handle) == "Privoxy")
                        {
                            WindowsUtility.HideWindow(handle);
                            started = true;
                            break;
                        }
                    }
                }
            });
        }

        public void Stop()
        {
            throw new System.NotImplementedException();
        }
    }
}
