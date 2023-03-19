using System;

namespace Knapcode.TorSharp.Tools
{
    internal static class EnumHelper
    {
        public static bool IsArm(this TorSharpArchitecture arch)
        {
            return arch == TorSharpArchitecture.Arm32 || arch == TorSharpArchitecture.Arm64;
        }
    }
}
