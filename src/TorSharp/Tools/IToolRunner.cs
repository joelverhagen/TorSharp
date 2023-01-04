using System;
using System.Threading.Tasks;

namespace Knapcode.TorSharp.Tools
{
    internal interface IToolRunner : IDisposable
    {
        Task StartAsync(Tool tool);
        void Stop();
        event EventHandler<DataEventArgs> Stdout;
        event EventHandler<DataEventArgs> Stderr;
    }
}