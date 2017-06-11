using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Knapcode.TorSharp.Tools
{
    public class SimpleToolRunner : IToolRunner, IDisposable
    {
        private static readonly Task CompletedTask = Task.FromResult(0);
        private bool _disposed;
        private readonly ConcurrentBag<Process> _processes = new ConcurrentBag<Process>();

        ~SimpleToolRunner()
        {
            Dispose(false);
        }

        public Task StartAsync(Tool tool)
        {
            // start the desired process
            var arguments = string.Join(" ", tool.Settings.GetArguments(tool));
            var startInfo = new ProcessStartInfo
            {
                FileName = tool.ExecutablePath,
                Arguments = arguments,
                WorkingDirectory = tool.WorkingDirectory,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
#if NET45
                WindowStyle = ProcessWindowStyle.Hidden,
#endif
            };

            Process process = Process.Start(startInfo);
            
            process.OutputDataReceived += (object sender, DataReceivedEventArgs e) =>
            {
                Console.WriteLine(e.Data);
            };

            process.ErrorDataReceived += (object sender, DataReceivedEventArgs e) =>
            {
                Console.Error.WriteLine(e.Data);
            };

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            if (process == null)
            {
                throw new TorSharpException($"Unable to start the process '{tool.ExecutablePath}'.");
            }

            _processes.Add(process);

            return CompletedTask;
        }

        public void Stop()
        {
            while (!_processes.IsEmpty)
            {
                Process process;
                _processes.TryTake(out process);

#if NET45
                // If the process has not yet exited, ask nicely first.
                if (!process.HasExited)
                {
                    if (process.CloseMainWindow())
                    {
                        process.WaitForExit(1000);
                    }
                }
#endif

                // Still not exited? Then it's no more Mr Nice Guy.
                if (!process.HasExited)
                {
                    process.Kill();
                    process.WaitForExit(1000);
                }
            }
        }

        void IDisposable.Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                try
                {
                    Stop();
                }
                catch
                {
                    // Not much can be done, but must stop this bubbling.
                }
            }

            // release any unmanaged objects
            // set the object references to null
            _disposed = true;
        }
    }
}
