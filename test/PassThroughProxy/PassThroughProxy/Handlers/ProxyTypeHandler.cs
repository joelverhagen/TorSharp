using System.IO;
using System.Text;
using System.Threading.Tasks;
using Proxy.Sessions;

namespace Proxy.Handlers
{
    public class ProxyTypeHandler : IHandler
    {
        private static readonly ProxyTypeHandler Self = new ProxyTypeHandler();

        private ProxyTypeHandler()
        {
        }

        public async Task<ExitReason> Run(SessionContext context)
        {
            var exitReason = context.Header.Verb == "CONNECT" ? ExitReason.HttpsTunnelRequired : ExitReason.HttpProxyRequired;

            if (exitReason == ExitReason.HttpProxyRequired && context.Configuration.Server.RejectHttpProxy)
            {
                await SendMethodNotAllowed(context.ClientStream);
                exitReason = ExitReason.TerminationRequired;
            }

            return exitReason;
        }

        public static ProxyTypeHandler Instance()
        {
            return Self;
        }

        private static async Task SendMethodNotAllowed(Stream stream)
        {
            var bytes = Encoding.ASCII.GetBytes("HTTP/1.1 405 Method Not Allowed\r\nConnection: close\r\n\r\n");
            await stream.WriteAsync(bytes, 0, bytes.Length);
        }
    }
}