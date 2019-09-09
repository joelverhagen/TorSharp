using System.Threading.Tasks;
using Proxy.Sessions;

namespace Proxy.Handlers
{
    public interface IHandler
    {
        Task<ExitReason> Run(SessionContext context);
    }
}