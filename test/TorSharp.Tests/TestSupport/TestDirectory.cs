using System;
using System.IO;
using Xunit.Abstractions;

namespace Knapcode.TorSharp.Tests.TestSupport
{
    public class TestDirectory : IDisposable
    {
        private readonly string _originalPath;
        private readonly ITestOutputHelper _output;

        public TestDirectory(ITestOutputHelper output)
        {
            _output = output;

            var guid = Guid.NewGuid().ToString("N");
            var path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "Knapcode.TorSharp.Tests", guid);
            _originalPath = path;
            Path = path;
        }

        public string Path { get; set; }

        public static implicit operator string(TestDirectory testDirectory) => testDirectory.Path;

        public void Dispose()
        {
            try
            {
                Directory.Delete(_originalPath, true);
            }
            catch (Exception e)
            {
                // not much we can do
                var error = $"Error when deleting when the test environment:{Environment.NewLine}{e}";
                if (_output == null)
                {
                    Console.Error.WriteLine(error);
                }
                else
                {
                    _output.WriteLine(error);
                }
            }
        }
    }
}