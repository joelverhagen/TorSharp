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

        private TestEnvironment(ITestOutputHelper output, TestDirectory testDirectory, ReservedPorts ports, bool setControlPassword)
        {
            _output = output;
            TestDirectory = testDirectory;

            if (setControlPassword)
            {
                _torControlPassword = Guid.NewGuid().ToString();
            }

            _ports = ports;
            _disposed = false;
        }

        public TestDirectory TestDirectory { get; }

        public TorSharpSettings BuildSettings()
        {
            ThrowIfDisposed();
            var settings = new TorSharpSettings
            {
                ZippedToolsDirectory = Path.Combine(TestDirectory.Path, "Zipped"),
                ExtractedToolsDirectory = Path.Combine(TestDirectory.Path, "Extracted"),
                PrivoxySettings =
                {
                    Port = _ports.Ports[0],
                },
                TorSettings =
                {
                    DataDirectory = Path.Combine(TestDirectory.Path, "TorData"),
                    SocksPort = _ports.Ports[1],
                    ControlPort = _ports.Ports[2],
                },
                ReloadTools = true,
                WriteToConsole = false,
            };

            if (_torControlPassword != null)
            {
                settings.TorSettings.ControlPassword = _torControlPassword;
            }

            return settings;
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _output.WriteLine("Disposing the test environment");
            _ports.Dispose();
            TestDirectory.Dispose();
            _disposed = true;
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException("The test environment has already been disposed.");
            }
        }

        public static TestEnvironment Initialize(ITestOutputHelper output, bool setControlPassword = true)
        {
            var testDirectory = new TestDirectory(output);
            output.WriteLine($"Initializing test environment in base directory: {testDirectory}");

            var ports = ReservedPorts.Reserve(3);
            output.WriteLine($"Reserved ports: {string.Join(", ", ports.Ports)}");

            Directory.CreateDirectory(testDirectory);

            return new TestEnvironment(output, testDirectory, ports, setControlPassword);
        }
    }
}