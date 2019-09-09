using System;

namespace Knapcode.TorSharp.Adapters
{
    internal interface IRandom : IDisposable
    {
        void GetBytes(byte[] bytes);
    }
}