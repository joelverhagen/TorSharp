using System.Threading.Tasks;

namespace Knapcode.TorSharp.Tools
{
    public interface IToolRunner
    {
        Task StartAsync(Tool tool);
        void Stop();
    }
}