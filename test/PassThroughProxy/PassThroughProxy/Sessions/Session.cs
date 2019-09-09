using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks;
using Proxy.Configurations;
using Proxy.Handlers;

namespace Proxy.Sessions
{
    public class Session
    {
        private static readonly Dictionary<ExitReason, IHandler> Handlers = new Dictionary<ExitReason, IHandler>
        {
            {ExitReason.InitializationRequired, FirstRequestHandler.Instance()},
            {ExitReason.Initialized, AuthenticationHandler.Instance()},
            {ExitReason.Authenticated, ProxyTypeHandler.Instance()},
            {ExitReason.AuthenticationNotRequired, ProxyTypeHandler.Instance()},
            {ExitReason.HttpProxyRequired, HttpProxyHandler.Instance()},
            {ExitReason.HttpsTunnelRequired, HttpsTunnelHandler.Instance()},
            {ExitReason.NewHostRequired, FirewallHandler.Instance()},
            {ExitReason.NewHostConnectionRequired, NewHostHandler.Instance()},
            {ExitReason.NewHostConnected, ProxyTypeHandler.Instance()}
        };

        public async Task Run(TcpClient client, Configuration configuration)
        {
            var result = ExitReason.InitializationRequired;

            using (var context = new SessionContext(client, configuration))
            {
                do
                {
                    try
                    {
                        result = await Handlers[result].Run(context);
                    }
                    catch (SocketException)
                    {
                        result = ExitReason.TerminationRequired;
                    }
                } while (result != ExitReason.TerminationRequired);
            }
        }
    }
}