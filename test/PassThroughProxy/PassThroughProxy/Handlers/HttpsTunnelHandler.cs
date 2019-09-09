using System.IO;
using System.Text;
using System.Threading.Tasks;
using Proxy.Sessions;
using Proxy.Tunnels;

namespace Proxy.Handlers
{
    public class HttpsTunnelHandler : IHandler
    {
        private static readonly HttpsTunnelHandler Self = new HttpsTunnelHandler();

        private HttpsTunnelHandler()
        {
        }

        public async Task<ExitReason> Run(SessionContext context)
        {
            if (context.CurrentHostAddress == null || !Equals(context.Header.Host, context.CurrentHostAddress))
            {
                return ExitReason.NewHostRequired;
            }

            using (var tunnel = new TcpTwoWayTunnel())
            {
                var task = tunnel.Run(context.ClientStream, context.HostStream);
                await SendConnectionEstablised(context.ClientStream);
                await task;
            }

            return ExitReason.TerminationRequired;
        }

        public static HttpsTunnelHandler Instance()
        {
            return Self;
        }

        private static async Task SendConnectionEstablised(Stream stream)
        {
            var bytes = Encoding.ASCII.GetBytes("HTTP/1.1 200 Connection established\r\n\r\n");
            await stream.WriteAsync(bytes, 0, bytes.Length);
        }
    }
}