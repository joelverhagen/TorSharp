using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Knapcode.TorSharp.Tools
{
    internal class SimpleToolRunner : IToolRunner
    {
        private readonly ConcurrentBag<Process> _processes = new ConcurrentBag<Process>();

        public event EventHandler<DataEventArgs> Stdout;
        public event EventHandler<DataEventArgs> Stderr;

        public Task StartAsync(Tool tool)
        {
            // start the desired process
            var arguments = string.Join(" ", tool.Settings.GetArguments(tool));
            var environmentVariables = tool.Settings.GetEnvironmentVariables(tool);
            var startInfo = new ProcessStartInfo
            {
                FileName = tool.ExecutablePath,
                Arguments = arguments,
                WorkingDirectory = tool.WorkingDirectory,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
            };

            foreach (var pair in environmentVariables)
            {
                startInfo.EnvironmentVariables[pair.Key] = pair.Value;
            }

            Process process = Process.Start(startInfo);
            
            process.OutputDataReceived += (object sender, DataReceivedEventArgs e) =>
            {
                Stdout?.Invoke(this, new DataEventArgs(tool.ExecutablePath, e.Data));
            };

            process.ErrorDataReceived += (object sender, DataReceivedEventArgs e) =>
            {
                Stderr?.Invoke(this, new DataEventArgs(tool.ExecutablePath, e.Data));
            };

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            if (process == null)
            {
                throw new TorSharpException($"Unable to start the process '{tool.ExecutablePath}'.");
            }

            _processes.Add(process);

            return Task.CompletedTask;
        }

        public void Stop()
        {
            while (!_processes.IsEmpty)
            {
                if (_processes.TryTake(out var process))
                {
                    using (process)
                    {
                        // If the process has not yet exited, ask nicely first.
                        if (!process.HasExited)
                        {
                            if (process.CloseMainWindow())
                            {
                                process.WaitForExit(1000);
                            }
                        }

                        // Still not exited? Then it's no more Mr Nice Guy.
                        if (!process.HasExited)
                        {
                            process.Kill();
                            process.WaitForExit(1000);
                        }
                    }
                }
            }
        }

        public void Dispose()
        {
            Stop();
        }
    }
}
