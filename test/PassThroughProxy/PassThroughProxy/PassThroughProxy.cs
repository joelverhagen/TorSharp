using System;
using System.Net.Sockets;
using System.Threading;
using Proxy.Configurations;
using Proxy.Listeners;
using Proxy.Sessions;

namespace Proxy
{
    public class PassThroughProxy : IDisposable
    {
        private ProxyListener _proxyListener;

        public PassThroughProxy(int listenPort) : this(listenPort, Configuration.Settings)
        {
        }

        public PassThroughProxy(Configuration configuration) : this(configuration.Server.Port, configuration)
        {
        }

        public PassThroughProxy(int listenPort, Configuration configuration)
        {
            Action<TcpClient, CancellationToken> handleClient =
                async (client, token) => await new Session().Run(client, configuration);

            _proxyListener = new ProxyListener(listenPort, handleClient);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_proxyListener != null)
                {
                    _proxyListener.Dispose();
                    _proxyListener = null;
                }
            }
        }
    }
}