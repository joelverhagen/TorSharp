using System;
using System.IO;
using Xunit.Abstractions;

namespace Knapcode.TorSharp.Tests.TestSupport
{
    public class TestEnvironment : IDisposable
    {
        private readonly string _torControlPassword;
        private readonly ReservedPorts _ports;
        private bool _disposed;
        private readonly ITestOutputHelper _output;
        private string _baseDirectory;
        private bool _deleteOnDispose;

        private TestEnvironment(ITestOutputHelper output, string torControlPassword, string baseDirectory, ReservedPorts ports)
        {
            _output = output;
            _baseDirectory = baseDirectory;
            _torControlPassword = torControlPassword;
            _ports = ports;
            _disposed = false;
            _deleteOnDispose = true;
        }

        public bool DeleteOnDispose
        {
            get { return _deleteOnDispose; }
            set
            {
                ThrowIfDisposed();
                _deleteOnDispose = value;
            }
        }

        public string BaseDirectory
        {
            get
            {
                ThrowIfDisposed();
                return _baseDirectory;
            }

            set
            {
                ThrowIfDisposed();
                _baseDirectory = value;
            }
        }

        public TorSharpSettings BuildSettings()
        {
            ThrowIfDisposed();
            return new TorSharpSettings
            {
                ZippedToolsDirectory = Path.Combine(BaseDirectory, "Zipped"),
                ExtractedToolsDirectory = Path.Combine(BaseDirectory, "Extracted"),
                PrivoxySettings =
                {
                    Port = _ports.Ports[0],
                },
                TorSettings =
                {
                    DataDirectory = Path.Combine(BaseDirectory, "TorData"),
                    SocksPort = _ports.Ports[1],
                    ControlPort = _ports.Ports[2],
                    ControlPassword = _torControlPassword,
                },
                ReloadTools = true
            };
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }
            
            _output.WriteLine("Disposing the test environment");

            _disposed = true;
            _ports.Dispose();

            if (_deleteOnDispose)
            {
                try
                {
                    Directory.Delete(_baseDirectory, true);
                }
                catch(Exception e)
                {
                    // not much we can do
                    _output.WriteLine($"Error when deleting when the test environment:{Environment.NewLine}{e}");
                }
            }
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException("The test environment has already been disposed.");
            }
        }

        public static TestEnvironment Initialize(ITestOutputHelper output)
        {
            var guid = Guid.NewGuid().ToString();
            var baseDirectory = Path.Combine(Path.GetTempPath(), "Knapcode.TorSharp.Tests", guid);
            output.WriteLine($"Initializing test environment in base directory: {baseDirectory}");

            var ports = ReservedPorts.Reserve(3);
            output.WriteLine($"Reserved ports: {string.Join(", ", ports.Ports)}");

            Directory.CreateDirectory(baseDirectory);

            return new TestEnvironment(output, guid, baseDirectory, ports);
        }
    }
}