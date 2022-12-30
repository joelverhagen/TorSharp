using System;
using System.Runtime.InteropServices;

namespace Knapcode.TorSharp.PInvoke
{
    internal static partial class WindowsApi
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

        [DllImport("user32.dll")]
        public static extern IntPtr CreateDesktop(string lpszDesktop, IntPtr lpszDevice, IntPtr pDevmode, int dwFlags, uint dwDesiredAccess, IntPtr lpsa);

        [DllImport("user32.dll")]
        public static extern bool CloseDesktop(IntPtr hDesktop);
    }

    internal static partial class WindowsUtility
    {
        public static SafeDesktopHandle CreateDesktop(string desktopName)
        {
            var result = WindowsApi.CreateDesktop(desktopName, IntPtr.Zero, IntPtr.Zero, 0, WindowsApi.GENERIC_ALL, IntPtr.Zero);
            if (result == IntPtr.Zero)
            {
                throw new TorSharpException($"Failed to create a virtual desktop. Error: {GetLastErrorMessage()}");
            }

            return new SafeDesktopHandle(result);
        }
    }
}