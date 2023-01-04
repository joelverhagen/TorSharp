using System;
using Microsoft.Win32.SafeHandles;

namespace Knapcode.TorSharp.PInvoke
{
    internal class SafeDesktopHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        public SafeDesktopHandle(IntPtr handle) : base(ownsHandle: true)
        {
            SetHandle(handle);
        }

        protected override bool ReleaseHandle()
        {
            WindowsApi.CloseDesktop(handle);
            return true;
        }
    }
}