using System;

namespace Knapcode.TorSharp.Adapters
{
    public interface IRandom : IDisposable
    {
        void GetBytes(byte[] bytes);
    }
}