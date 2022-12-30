using System;
using Microsoft.Win32.SafeHandles;

namespace Knapcode.TorSharp.PInvoke
{
    internal class SafeJobHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        public SafeJobHandle(IntPtr handle) : base(ownsHandle: true)
        {
            SetHandle(handle);
        }

        protected override bool ReleaseHandle()
        {
            if (!WindowsApi.TerminateJobObject(handle, uExitCode: 0))
            {
                throw new TorSharpException($"Unable to terminate the job object. Error: {WindowsUtility.GetLastErrorMessage()}");
            }

            return true;
        }
    }
}
