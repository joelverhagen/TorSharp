using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Knapcode.TorSharp.PInvoke
{
    internal static partial class WindowsApi
    {
        public const int SW_SHOW = 5;
        public const int SW_HIDE = 0;
        public const int GWL_STYLE = (-16);
        public const uint WS_VISIBLE = 0x10000000;
        public const uint WS_EX_TOOLWINDOW = 0x0080;
        public const uint WS_EX_APPWINDOW = 0x40000;

        public delegate bool EnumThreadWndProc(IntPtr hWnd, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern bool EnumThreadWindows(int dwThreadId, EnumThreadWndProc lpfn, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll")]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll")]
        public static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);
    }

    internal static partial class WindowsUtility
    {
        public static void HideWindow(IntPtr windowHandle)
        {
            // Source: http://stackoverflow.com/a/7219089/52749
            long style = WindowsApi.GetWindowLong(windowHandle, WindowsApi.GWL_STYLE);
            style &= ~(WindowsApi.WS_VISIBLE);
            style |= WindowsApi.WS_EX_TOOLWINDOW;
            style &= ~(WindowsApi.WS_EX_APPWINDOW);
            WindowsApi.ShowWindow(windowHandle, WindowsApi.SW_HIDE);
            WindowsApi.SetWindowLong(windowHandle, WindowsApi.GWL_STYLE, (int)style);
            WindowsApi.ShowWindow(windowHandle, WindowsApi.SW_SHOW);
            WindowsApi.ShowWindow(windowHandle, WindowsApi.SW_HIDE);
        }

        public static string GetWindowText(IntPtr windowHandle)
        {
            int length = WindowsApi.GetWindowTextLength(windowHandle) + 1;
            var stringBuilder = new StringBuilder(length);
            WindowsApi.GetWindowText(windowHandle, stringBuilder, length);
            return stringBuilder.ToString();
        }

        public static IEnumerable<IntPtr> GetWindowHandles(int processId)
        {
            var handles = new List<IntPtr>();
            foreach (var thread in Process.GetProcessById(processId).Threads.OfType<ProcessThread>())
            {
                WindowsApi.EnumThreadWindows(thread.Id, (h, p) => { handles.Add(h); return true; }, IntPtr.Zero);
            }

            return handles;
        }
    }
}
