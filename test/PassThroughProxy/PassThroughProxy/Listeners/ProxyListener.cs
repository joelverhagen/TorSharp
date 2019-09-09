using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Proxy.Listeners
{
    public class ProxyListener : IDisposable
    {
        private TcpListener _listener;
        private CancellationTokenSource _source;

        public ProxyListener(int port, Action<TcpClient, CancellationToken> handleClient)
        {
            _listener = new TcpListener(IPAddress.Any, port);
            _source = new CancellationTokenSource();

            _listener.Start();
            AcceptClients(_listener, handleClient, _source.Token);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private static async void AcceptClients(TcpListener listener, Action<TcpClient, CancellationToken> handleClient, CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                var tcpClient = await listener.AcceptTcpClientAsync();

                handleClient(tcpClient, token);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_source != null)
                {
                    _source.Cancel();
                    _source.Dispose();
                    _source = null;
                }

                if (_listener != null)
                {
                    _listener.Stop();
                    _listener = null;
                }
            }
        }
    }
}