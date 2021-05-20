using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace UnsharedNamespace
{
    public class PublicTypesTests
    {
        [Fact]
        public void TheSetOfPublicTypesIsExpected()
        {
            var expectedTypes = new List<Type>()
            {
                typeof(Knapcode.TorSharp.ITorSharpProxy),
                typeof(Knapcode.TorSharp.ITorSharpToolFetcher),
                typeof(Knapcode.TorSharp.ToolDownloadStrategy),
                typeof(Knapcode.TorSharp.ToolRunnerType),
                typeof(Knapcode.TorSharp.Tools.DownloadableFile),
                typeof(Knapcode.TorSharp.Tools.Tor.TorControlClient),
                typeof(Knapcode.TorSharp.Tools.Tor.TorControlException),
                typeof(Knapcode.TorSharp.Tools.ZippedToolFormat),
                typeof(Knapcode.TorSharp.ToolUpdate),
                typeof(Knapcode.TorSharp.ToolUpdates),
                typeof(Knapcode.TorSharp.ToolUpdateStatus),
                typeof(Knapcode.TorSharp.TorSharpArchitecture),
                typeof(Knapcode.TorSharp.TorSharpException),
                typeof(Knapcode.TorSharp.TorSharpOSPlatform),
                typeof(Knapcode.TorSharp.TorSharpPrivoxySettings),
                typeof(Knapcode.TorSharp.TorSharpProxy),
                typeof(Knapcode.TorSharp.TorSharpProxyExtensions),
                typeof(Knapcode.TorSharp.TorSharpSettings),
                typeof(Knapcode.TorSharp.TorSharpToolFetcher),
                typeof(Knapcode.TorSharp.TorSharpTorSettings),
            };

            var publicTypes = typeof(Knapcode.TorSharp.TorSharpProxy)
                .Assembly
                .GetTypes()
                .Where(x => x.IsPublic)
                .ToList();

            Assert.Empty(publicTypes.Except(expectedTypes));
            Assert.Empty(expectedTypes.Except(publicTypes));
        }
    }
}
