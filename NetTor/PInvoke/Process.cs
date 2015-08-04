using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Knapcode.NetTor.PInvoke
{
    public static partial class WindowsApi
    {
        public const uint INFINITE = 0xFFFFFFFF;
        public const uint WAIT_ABANDONED = 0x00000080;
        public const uint WAIT_TIMEOUT = 0x00000102;

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

    public static partial class WindowsUtility
    {
        public static WindowsApi.PROCESS_INFORMATION CreateProcess(ProcessStartInfo startInfo, string desktopName = null, int? millisecondsToWait = 100)
        {
            var startupInfo = new WindowsApi.STARTUPINFO();
            startupInfo.cb = Marshal.SizeOf(startupInfo);
            startupInfo.lpDesktop = desktopName;

            var processInformation = new WindowsApi.PROCESS_INFORMATION();

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
                    WindowsApi.WaitForInputIdle(processInformation.hProcess, (uint) millisecondsToWait.Value);
                }

                WindowsApi.CloseHandle(processInformation.hThread);
                return processInformation;
            }

            return new WindowsApi.PROCESS_INFORMATION();
        }

        public static string GetLastErrorMessage()
        {
            return new Win32Exception(Marshal.GetLastWin32Error()).Message;
        }
    }
}
