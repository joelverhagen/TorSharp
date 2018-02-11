using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading;

namespace Knapcode.TorSharp.Tests.TestSupport
{
    public class ReservedPort : IDisposable
    {
        private static readonly object Lock = new object();
        private static readonly HashSet<int> ReservedPorts = new HashSet<int>();

        private readonly int _port;
        private readonly Semaphore _semaphore;
        private bool _disposed;

        private ReservedPort(int port, Semaphore semaphore)
        {
            _port = port;
            _semaphore = semaphore;
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
                
                var semaphore = new Semaphore(1, 1, $@"Global\Knapcode.TorSharp.Tests.ReservedPort-{port}");
                lock (Lock)
                {
                    if (ReservedPorts.Contains(port))
                    {
                        continue;
                    }

                    var acquired = semaphore.WaitOne(TimeSpan.Zero);
                    if (acquired)
                    {
                        ReservedPorts.Add(port);
                        return new ReservedPort(port, semaphore);
                    }
                }
            }

            return null;
        }

        private static bool IsPortFree(int port)
        {
            var collision = IPGlobalProperties
                .GetIPGlobalProperties()
                .GetActiveTcpListeners()
                .Select(e => e.Port)
                .Contains(port);

            return !collision;
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _semaphore.Release();
            _semaphore.Dispose();
            lock (Lock)
            {
                ReservedPorts.Remove(_port);
            }
        }
    }
}