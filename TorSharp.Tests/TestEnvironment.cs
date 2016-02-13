using System;
using System.IO;

namespace Knapcode.TorSharp.Tests
{
    public class TestEnvironment : IDisposable
    {
        private readonly string _torControlPassword;
        private readonly ReservedPorts _ports;
        private bool _disposed;
        private readonly string _baseDirectory;

        private TestEnvironment(string torControlPassword, string baseDirectory, ReservedPorts ports)
        {
            _baseDirectory = baseDirectory;
            _torControlPassword = torControlPassword;
            _ports = ports;
            _disposed = false;
        }

        public string BaseDirectory
        {
            get
            {
                ThrowIfDisposed();
                return _baseDirectory;
            }
        }

        public TorSharpSettings BuildSettings()
        {
            ThrowIfDisposed();
            return new TorSharpSettings
            {
                ZippedToolsDirectory = Path.Combine(BaseDirectory, "Zipped"),
                ExtractedToolsDirectory = Path.Combine(BaseDirectory, "Extracted"),
                TorDataDirectory = Path.Combine(BaseDirectory, "TorData"),
                PrivoxyPort = _ports.Ports[0],
                TorSocksPort = _ports.Ports[1],
                TorControlPort = _ports.Ports[2],
                TorControlPassword = _torControlPassword,
                ReloadTools = true
            };
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _ports.Dispose();

            try
            {
                Directory.Delete(_baseDirectory, true);
            }
            catch
            {
                // not much we can do
            }
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException("The test environment has already been disposed.");
            }
        }

        public static TestEnvironment Initialize()
        {
            var guid = Guid.NewGuid().ToString();
            var baseDirectory = Path.Combine(Path.GetTempPath(), "Knapcode.TorSharp.Tests", guid);

            var ports = ReservedPorts.Reserve(3);
            Directory.CreateDirectory(baseDirectory);

            return new TestEnvironment(guid, baseDirectory, ports);
        }
    }
}