using System;

namespace Knapcode.TorSharp
{
    /// <summary>
    /// CPU architectures that TorSharp can run on.
    /// </summary>
    [Flags]
    public enum TorSharpArchitecture
    {
        Unknown = 0,
        X86 = 1,
        X64 = 2,
        Arm = 4,
        Arm32 = Arm | X86,
        Arm64 = Arm | X64
    }
}