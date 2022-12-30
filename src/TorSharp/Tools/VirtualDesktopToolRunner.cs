using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Knapcode.TorSharp.PInvoke;
using Microsoft.Win32.SafeHandles;

namespace Knapcode.TorSharp.Tools
{
    internal class VirtualDesktopToolRunner : IToolRunner
    {
        private readonly object _jobHandleLock = new object();
        private SafeJobHandle _jobHandle = new SafeJobHandle(IntPtr.Zero);
        private readonly TorSharpSettings _settings;
        private readonly object _processIdsLock = new object();
        private readonly HashSet<int> _processIds = new HashSet<int>();
        private readonly ConcurrentQueue<IDisposable> _disposables = new ConcurrentQueue<IDisposable>();

        public event EventHandler<DataEventArgs> Stdout;
        public event EventHandler<DataEventArgs> Stderr;

        public VirtualDesktopToolRunner(TorSharpSettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));

            if (_settings.VirtualDesktopName == null)
            {
                using (var hash = SHA256.Create())
                {
                    var pathBytes = Encoding.UTF8.GetBytes(_settings.ExtractedToolsDirectory);
                    var pathHash = hash.ComputeHash(pathBytes);
                    _settings.VirtualDesktopName = "TorSharpDesktop-" + Convert.ToBase64String(pathHash);
                }
            }
        }

        public Task StartAsync(Tool tool)
        {
            var desktopHandle = WindowsUtility.CreateDesktop(_settings.VirtualDesktopName);
            _disposables.Enqueue(desktopHandle);

            // embrace the madness -- it seems Windows always wants exactly one instance of "ctfmon.exe" in the new desktop
            var ctfmonStartInfo = new ProcessStartInfo
            {
                FileName = "ctfmon.exe",
                WorkingDirectory = "."
            };
            var ctfmonProcess = WindowsUtility.CreateProcess(ctfmonStartInfo, _settings.VirtualDesktopName, throwOnError: false);
            if (ctfmonProcess != null)
            {
                AssociateWithJob(ctfmonProcess.ProcessInformation, throwOnError: false);
            }

            // start the desired process
            var arguments = string.Join(" ", tool.Settings.GetArguments(tool));
            var startInfo = new ProcessStartInfo
            {
                FileName = tool.ExecutablePath,
                Arguments = arguments,
                WorkingDirectory = tool.WorkingDirectory
            };
            var targetProcess = WindowsUtility.CreateProcess(
                startInfo,
                _settings.VirtualDesktopName,
                onStdout: x => Stdout?.Invoke(this, new DataEventArgs(tool.ExecutablePath, x)),
                onStderr: x => Stderr?.Invoke(this, new DataEventArgs(tool.ExecutablePath, x)));
            AssociateWithJob(targetProcess.ProcessInformation, throwOnError: true);

            return Task.CompletedTask;
        }

        private void AssociateWithJob(WindowsApi.PROCESS_INFORMATION processInformation, bool throwOnError)
        {
            if (_jobHandle.IsInvalid || _jobHandle.IsClosed)
            {
                lock (_jobHandleLock)
                {
                    if (_jobHandle.IsInvalid || _jobHandle.IsClosed)
                    {
                        var handle = WindowsApi.CreateJobObject(IntPtr.Zero, null);
                        if (handle == IntPtr.Zero)
                        {
                            throw new TorSharpException($"Unable to create the job object. Error: {WindowsUtility.GetLastErrorMessage()}");
                        }

                        var jobHandle = new SafeJobHandle(handle);

                        var basic = new WindowsApi.JOBOBJECT_BASIC_LIMIT_INFORMATION { LimitFlags = WindowsApi.JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE };
                        var extended = new WindowsApi.JOBOBJECT_EXTENDED_LIMIT_INFORMATION { BasicLimitInformation = basic };

                        int length = Marshal.SizeOf(typeof(WindowsApi.JOBOBJECT_EXTENDED_LIMIT_INFORMATION));
                        IntPtr extendedPointer = Marshal.AllocHGlobal(length);
                        Marshal.StructureToPtr(extended, extendedPointer, false);

                        if (!WindowsApi.SetInformationJobObject(jobHandle.DangerousGetHandle(), WindowsApi.JOBOBJECTINFOCLASS.ExtendedLimitInformation, extendedPointer, (uint)length))
                        {
                            throw new TorSharpException($"Unable to set information on the job object. Error: {WindowsUtility.GetLastErrorMessage()}");
                        }

                        _jobHandle = jobHandle;
                    }
                }
            }

            _disposables.Enqueue(new SafeProcessHandle(processInformation.hProcess, ownsHandle: true));
            _disposables.Enqueue(new SafeProcessHandle(processInformation.hThread, ownsHandle: true));

            lock (_processIdsLock)
            {
                _processIds.Add(processInformation.dwProcessId);
            }

            if (!WindowsApi.AssignProcessToJobObject(_jobHandle.DangerousGetHandle(), processInformation.hProcess) && throwOnError)
            {
                throw new TorSharpException($"Unable to assign the process to the job object. Error: {WindowsUtility.GetLastErrorMessage()}");
            }
        }

        public void Stop()
        {
            if (!_jobHandle.IsInvalid && !_jobHandle.IsClosed)
            {
                lock (_jobHandleLock)
                {
                    if (!_jobHandle.IsInvalid && !_jobHandle.IsClosed)
                    {
                        _jobHandle.Dispose();

                        // wait for all jobs to complete
                        lock (_processIdsLock)
                        {
                            var firstAttempt = true;
                            while (_processIds.Any())
                            {
                                if (!firstAttempt)
                                {
                                    Thread.Sleep(100);
                                }

                                firstAttempt = false;

                                _processIds.IntersectWith(Process
                                    .GetProcesses()
                                    .Select(x => x.Id));
                            }
                        }
                    }
                }
            }

            while (_disposables.TryDequeue(out var disposable))
            {
                disposable.Dispose();
            }
        }

        public void Dispose()
        {
            Stop();
        }
    }
}