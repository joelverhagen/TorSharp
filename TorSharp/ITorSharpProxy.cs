using System;
using System.Threading.Tasks;

namespace Knapcode.TorSharp
{
    public interface ITorSharpProxy : IDisposable
    {
        Task ConfigureAndStartAsync();
        Task GetNewIdentityAsync();
        void Stop();
    }
}
