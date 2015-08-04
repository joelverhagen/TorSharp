using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Knapcode.TorSharp.PInvoke
{
    public static partial class WindowsApi
    {
        public const uint DELETE = 0x00010000;
        public const uint READ_CONTROL = 0x00020000;
        public const uint WRITE_DAC = 0x00040000;
        public const uint WRITE_OWNER = 0x00080000;
        public const uint STANDARD_RIGHTS_REQUIRED =
            DELETE |
            READ_CONTROL |
            WRITE_DAC |
            WRITE_OWNER;
        public const uint DESKTOP_CREATEMENU = 0x0004;
        public const uint DESKTOP_CREATEWINDOW = 0x0002;
        public const uint DESKTOP_ENUMERATE = 0x0040;
        public const uint DESKTOP_HOOKCONTROL = 0x0008;
        public const uint DESKTOP_JOURNALPLAYBACK = 0x0020;
        public const uint DESKTOP_JOURNALRECORD = 0x0010;
        public const uint DESKTOP_READOBJECTS = 0x0001;
        public const uint DESKTOP_SWITCHDESKTOP = 0x0100;
        public const uint DESKTOP_WRITEOBJECTS = 0x0080;
        public const uint GENERIC_ALL =
            DESKTOP_CREATEMENU |
            DESKTOP_CREATEWINDOW |
            DESKTOP_ENUMERATE |
            DESKTOP_HOOKCONTROL |
            DESKTOP_JOURNALPLAYBACK |
            DESKTOP_JOURNALRECORD |
            DESKTOP_READOBJECTS |
            DESKTOP_SWITCHDESKTOP |
            DESKTOP_WRITEOBJECTS |
            STANDARD_RIGHTS_REQUIRED;
        public const int NORMAL_PRIORITY_CLASS = 0x00000020;

        public const int UOI_FLAGS = 1;
        public const int UOI_NAME = 2;
        public const int UOI_TYPE = 3;
        public const int UOI_USER_SID = 4;
        public const int UOI_HEAPSIZE = 5;
        public const int UOI_IO = 6;   

        public delegate bool EnumDesktopProc(string lpszDesktop, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern IntPtr GetProcessWindowStation();

        [DllImport("user32.dll")]
        public static extern IntPtr GetThreadDesktop(int dwThreadId);

        [DllImport("user32.dll")]
        public static extern bool EnumDesktops(IntPtr hwinsta, EnumDesktopProc lpEnumFunc, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern bool GetUserObjectInformation(IntPtr hObj, int nIndex, StringBuilder pvInfo, uint nLength, out uint plnLengthNeeded);

        [DllImport("user32.dll")]
        public static extern IntPtr CreateDesktop(string lpszDesktop, IntPtr lpszDevice, IntPtr pDevmode, int dwFlags, uint dwDesiredAccess, IntPtr lpsa);

        [DllImport("user32.dll")]
        public static extern IntPtr OpenDesktop(string lpszDesktop, int dwFlags, bool fInherit, uint dwDesiredAccess);

        [DllImport("user32.dll")]
        public static extern IntPtr OpenInputDesktop(int dwFlags, bool fInherit, uint dwDesiredAccess);

        [DllImport("user32.dll")]
        public static extern IntPtr EnumDesktopWindows(IntPtr hDesktop, EnumThreadWndProc lpfn, IntPtr lParam);
        
        [DllImport("user32.dll")]
        public static extern bool CloseDesktop(IntPtr hDesktop);

        [DllImport("user32.dll")]
        public static extern bool SwitchDesktop(IntPtr hDesktop);
    }

    public static partial class WindowsUtility
    {
        public static IntPtr CreateDesktop(string desktopName)
        {
            return WindowsApi.CreateDesktop(desktopName, IntPtr.Zero, IntPtr.Zero, 0, WindowsApi.GENERIC_ALL, IntPtr.Zero);
        }

        public static string GetDesktopName(IntPtr desktopHandle)
        {
            uint length;
            WindowsApi.GetUserObjectInformation(desktopHandle, WindowsApi.UOI_NAME, null, 0, out length);
            var stringBuilder = new StringBuilder((int) length + 1);
            WindowsApi.GetUserObjectInformation(desktopHandle, WindowsApi.UOI_NAME, stringBuilder, length, out length);
            return stringBuilder.ToString();
        }

        public static IntPtr OpenDesktop(string desktopName)
        {
            return WindowsApi.OpenDesktop(desktopName, 0, true, WindowsApi.GENERIC_ALL);
        }

        public static IntPtr OpenInputDesktop()
        {
            return WindowsApi.OpenInputDesktop(0, true, WindowsApi.GENERIC_ALL);
        }

        public static IEnumerable<string> GetDesktopNames()
        {
            var windowStationHandle = WindowsApi.GetProcessWindowStation();
            return GetDesktopNames(windowStationHandle);
        }

        public static IEnumerable<string> GetDesktopNames(IntPtr windowStationHandle)
        {
            var names = new List<string>();
            WindowsApi.EnumDesktops(windowStationHandle, (n, p) => { names.Add(n); return true; }, IntPtr.Zero);
            return names;
        }

        public static IEnumerable<IntPtr> GetDesktopWindowHandles(IntPtr desktopHandle)
        {
            var handles = new List<IntPtr>();
            WindowsApi.EnumDesktopWindows(desktopHandle, (d, p) => { handles.Add(d); return true; }, IntPtr.Zero);
            return handles;
        }

        public static IEnumerable<uint> GetDesktopProcessIds(IntPtr desktopHandle)
        {
            var pids = new HashSet<uint>();
            foreach (var windowHandle in GetDesktopWindowHandles(desktopHandle))
            {
                uint pid;
                WindowsApi.GetWindowThreadProcessId(windowHandle, out pid);
                pids.Add(pid);
            }

            return pids;
        }
    }
}