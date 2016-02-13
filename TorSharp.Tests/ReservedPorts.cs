using System;
using System.Linq;

namespace Knapcode.TorSharp.Tests
{
    public class ReservedPorts : IDisposable
    {
        private readonly ReservedPort[] _ports;
        private bool _disposed;

        public ReservedPorts(ReservedPort[] ports)
        {
            _ports = ports;
            _disposed = false;
        }

        public int[] Ports
        {
            get
            {
                if (_disposed)
                {
                    throw new ObjectDisposedException("The ports are not longer reserved.");
                }

                return _ports.Select(p => p.Port).ToArray();
            }
        }

        public static ReservedPorts Reserve(int count)
        {
            // reserve all of the ports
            var ports = new ReservedPort[count];
            try
            {
                for (int i = 0; i < count; i++)
                {
                    ports[i] = ReservedPort.Reserve();
                }
            }
            catch
            {
                DisposePorts(ports);
                throw;
            }

            return new ReservedPorts(ports);
        }

        private static void DisposePorts(ReservedPort[] ports)
        {
            foreach (var port in ports)
            {
                try
                {
                    port?.Dispose();
                }
                catch (Exception)
                {
                    // not much can be done about this
                }
            }
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            DisposePorts(_ports);
        }
    }
}