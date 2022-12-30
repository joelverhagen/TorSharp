using System;
using System.IO;

namespace Knapcode.TorSharp.Tests.TestSupport
{
    public class TestAppFixture : IDisposable
    {
        public TestAppFixture()
        {
            SharedDirectory = new TestDirectory(output: null);
            Directory.CreateDirectory(SharedDirectory);
        }

        public TestDirectory SharedDirectory { get; }

        public void Dispose()
        {
            SharedDirectory.Dispose();
        }
    }
}
