using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace Knapcode.TorSharp.Tests.TestSupport
{
    public class ReservedPort : IDisposable
    {
        private static readonly object Lock = new object();
        private static readonly HashSet<int> ReservedPorts = new HashSet<int>();

        private readonly int _port;
        private bool _disposed;

        private ReservedPort(int port)
        {
            _port = port;
            _disposed = false;
        }

        public int Port
        {
            get
            {
                if (_disposed)
                {
                    throw new ObjectDisposedException("The port is not longer reserved.");
                }

                return _port;
            }
        }

        public static ReservedPort Reserve()
        {
            var port = 50000;
            while (port < ushort.MaxValue)
            {
                port++;
                
                if (!IsPortFree(port))
                {
                    continue;
                }
                
                lock (Lock)
                {
                    if (ReservedPorts.Contains(port))
                    {
                        continue;
                    }

                    ReservedPorts.Add(port);
                    return new ReservedPort(port);
                }
            }

            return null;
        }

        private static bool IsPortFree(int port)
        {
            var tcpListener = new TcpListener(IPAddress.Loopback, port);
            try
            {
                tcpListener.Start();
                return true;
            }
            catch (SocketException)
            {
                return false;
            }
            finally
            {
                tcpListener.Stop();
            }
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            lock (Lock)
            {
                ReservedPorts.Remove(_port);
            }
        }
    }
}