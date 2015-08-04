using System.Threading.Tasks;

namespace Knapcode.NetTor.Tools
{
    public interface IToolRunner
    {
        Task StartAsync(Tool tool);
        void Stop();
    }
}