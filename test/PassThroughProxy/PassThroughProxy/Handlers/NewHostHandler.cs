using System.Net.Sockets;
using System.Threading.Tasks;
using Proxy.Headers;
using Proxy.Sessions;

namespace Proxy.Handlers
{
    public class NewHostHandler : IHandler
    {
        private static readonly NewHostHandler Self = new NewHostHandler();

        private NewHostHandler()
        {
        }

        public async Task<ExitReason> Run(SessionContext context)
        {
            context.RemoveHost();

            context.AddHost(await Connect(context.Header));

            context.CurrentHostAddress = context.Header.Host;

            return ExitReason.NewHostConnected;
        }

        public static NewHostHandler Instance()
        {
            return Self;
        }

        private static async Task<TcpClient> Connect(HttpHeader httpHeader)
        {
            var host = new TcpClient();
            await host.ConnectAsync(httpHeader.Host.Hostname, httpHeader.Host.Port);
            return host;
        }
    }
}