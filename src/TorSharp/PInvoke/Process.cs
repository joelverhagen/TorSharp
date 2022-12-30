using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Knapcode.TorSharp.PInvoke
{
    internal static partial class WindowsApi
    {
        public const int HANDLE_FLAG_INHERIT = 0x00000001;
        public const int STARTF_USESTDHANDLES = 0x00000100;
        public const int NORMAL_PRIORITY_CLASS = 0x00000020;

        [DllImport("kernel32.dll")]
        public static extern bool CloseHandle(IntPtr hObject);

        [DllImport("user32.dll")]
        public static extern uint WaitForInputIdle(IntPtr hProcess, uint dwMilliseconds);

        [DllImport("kernel32.dll")]
        public static extern bool CreateProcess(
            string lpApplicationName,
            string lpCommandLine,
            IntPtr lpProcessAttributes,
            IntPtr lpThreadAttributes,
            bool bInheritHandles,
            int dwCreationFlags,
            IntPtr lpEnvironment,
            string lpCurrentDirectory,
            ref STARTUPINFO lpStartupInfo,
            ref PROCESS_INFORMATION lpProcessInformation);

        [DllImport("kernel32.dll")]
        public static extern bool CreatePipe(ref IntPtr hReadPipe, ref IntPtr hWritePipe, ref SECURITY_ATTRIBUTES lpPipeAttributes, int nSize);

        [DllImport("kernel32.dll")]
        public static extern bool SetHandleInformation(IntPtr hObject, int dwMask, int dwFlags);

        [StructLayout(LayoutKind.Sequential)]
        public struct PROCESS_INFORMATION
        {
            public IntPtr hProcess;
            public IntPtr hThread;
            public int dwProcessId;
            public int dwThreadId;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct STARTUPINFO
        {
            public int cb;
            public string lpReserved;
            public string lpDesktop;
            public string lpTitle;
            public int dwX;
            public int dwY;
            public int dwXSize;
            public int dwYSize;
            public int dwXCountChars;
            public int dwYCountChars;
            public int dwFillAttribute;
            public int dwFlags;
            public short wShowWindow;
            public short cbReserved2;
            public IntPtr lpReserved2;
            public IntPtr hStdInput;
            public IntPtr hStdOutput;
            public IntPtr hStdError;
        }
    }

    internal static partial class WindowsUtility
    {
        public static RedirectedProcess CreateProcess(
            ProcessStartInfo startInfo,
            string desktopName = null,
            int? millisecondsToWait = 100,
            bool throwOnError = true,
            Action<string> onStdout = null,
            Action<string> onStderr = null)
        {
            var startupInfo = new WindowsApi.STARTUPINFO();
            startupInfo.cb = Marshal.SizeOf(startupInfo);
            startupInfo.lpDesktop = desktopName;
            startupInfo.dwFlags = WindowsApi.STARTF_USESTDHANDLES;

            var parentStdout = IntPtr.Zero;
            var childStdout = IntPtr.Zero;
            var parentStderr = IntPtr.Zero;
            var childStderr = IntPtr.Zero;
            var processInformation = new WindowsApi.PROCESS_INFORMATION();
            FileStreamEventEmitter stdout = null;
            FileStreamEventEmitter stderr = null;

            try
            {
                if (onStdout != null)
                {
                    CreatePipe(out parentStdout, out childStdout);
                    startupInfo.hStdOutput = childStdout;
                }

                if (onStderr != null)
                {
                    CreatePipe(out parentStderr, out childStderr);
                    startupInfo.hStdError = childStderr;
                }

                string command = startInfo.FileName + " " + startInfo.Arguments;

                bool result = WindowsApi.CreateProcess(null,
                    command,
                    IntPtr.Zero,
                    IntPtr.Zero,
                    true,
                    WindowsApi.NORMAL_PRIORITY_CLASS,
                    IntPtr.Zero,
                    startInfo.WorkingDirectory,
                    ref startupInfo,
                    ref processInformation);

                if (result)
                {
                    if (millisecondsToWait.HasValue)
                    {
                        WindowsApi.WaitForInputIdle(processInformation.hProcess, (uint)millisecondsToWait.Value);
                    }

                    if (onStdout != null)
                    {
                        stdout = new FileStreamEventEmitter(parentStdout, onStdout);
                    }

                    if (onStderr != null)
                    {
                        stderr = new FileStreamEventEmitter(parentStdout, onStderr);
                    }

                    return new RedirectedProcess(processInformation, stdout, stderr);
                }
                else if (throwOnError)
                {
                    throw new TorSharpException($"Failed to start a process with command '{command}'. Error: {GetLastErrorMessage()}");
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                CloseHandle(parentStdout);
                CloseHandle(parentStderr);
                CloseHandle(processInformation.hProcess);
                throw;
            }
            finally
            {
                CloseHandle(childStdout);
                CloseHandle(childStderr);
            }
        }

        internal static void CreatePipe(out IntPtr readHandle, out IntPtr writeHandle)
        {
            readHandle = IntPtr.Zero;
            writeHandle = IntPtr.Zero;
            var securityAttributes = new WindowsApi.SECURITY_ATTRIBUTES();
            securityAttributes.bInheritHandle = true;

            if (!WindowsApi.CreatePipe(ref readHandle, ref writeHandle, ref securityAttributes, nSize: 0))
            {
                throw new TorSharpException($"Failed to create a pipe. Error: {GetLastErrorMessage()}");
            }

            // Source: https://learn.microsoft.com/en-us/windows/win32/api/handleapi/nf-handleapi-sethandleinformation
            const int dwMask = WindowsApi.HANDLE_FLAG_INHERIT;
            if (!WindowsApi.SetHandleInformation(readHandle, dwMask, dwFlags: 0))
            {
                throw new TorSharpException($"Failed to set handle information on a pipe. Error: {GetLastErrorMessage()}");
            }
        }

        internal static void CloseHandle(IntPtr handle)
        {
            if (handle != IntPtr.Zero)
            {
                WindowsApi.CloseHandle(handle);
            }
        }

        public static string GetLastErrorMessage()
        {
            return new Win32Exception(Marshal.GetLastWin32Error()).Message;
        }
    }

    internal class RedirectedProcess
    {
        public RedirectedProcess(WindowsApi.PROCESS_INFORMATION process, FileStreamEventEmitter stdout, FileStreamEventEmitter stderr)
        {
            ProcessInformation = process;
            Stdout = stdout;
            Stderr = stderr;
        }

        public WindowsApi.PROCESS_INFORMATION ProcessInformation { get; }
        public FileStreamEventEmitter Stdout { get; }
        public FileStreamEventEmitter Stderr { get; }
    }
}
