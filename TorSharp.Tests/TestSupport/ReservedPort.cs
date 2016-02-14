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
        private readonly Mutex _mutex;
        private bool _disposed;

        private ReservedPort(int port, Mutex mutex)
        {
            _port = port;
            _mutex = mutex;
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

                var collision = IPGlobalProperties
                    .GetIPGlobalProperties()
                    .GetActiveTcpListeners()
                    .Select(e => e.Port)
                    .Contains(port);

                if (collision)
                {
                    continue;
                }

                bool createdNew;
                Mutex mutex = new Mutex(false, $@"Global\Knapcode.TorSharp.Tests.ReservedPort-{port}", out createdNew);
                lock (Lock)
                {
                    if (ReservedPorts.Contains(port))
                    {
                        continue;
                    }

                    var acquired = mutex.WaitOne(0, false);
                    if (acquired)
                    {
                        ReservedPorts.Add(port);
                        return new ReservedPort(port, mutex);
                    }
                }
            }

            return null;
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _mutex.ReleaseMutex();
            _mutex.Dispose();
            lock (Lock)
            {
                ReservedPorts.Remove(_port);
            }
        }
    }
}