using System;
using System.Collections.Generic;
using System.Linq;
using Knapcode.TorSharp.Tools;
using Knapcode.TorSharp.Tools.Privoxy;
using Knapcode.TorSharp.Tools.Tor;
using Xunit;

namespace Knapcode.TorSharp.Tests
{
    public class PublicTypesTests
    {
        [Fact]
        public void TheSetOfPublicTypesIsExpected()
        {
            var expectedTypes = new List<Type>()
            {
                typeof(ITorSharpProxy),
                typeof(ITorSharpToolFetcher),
                typeof(ToolDownloadStrategy),
                typeof(ToolRunnerType),
                typeof(DownloadableFile),
                typeof(PrivoxySettings),
                typeof(TorControlClient),
                typeof(TorControlException),
                typeof(TorSettings),
                typeof(ZippedToolFormat),
                typeof(ToolUpdate),
                typeof(ToolUpdates),
                typeof(ToolUpdateStatus),
                typeof(TorSharpArchitecture),
                typeof(TorSharpException),
                typeof(TorSharpOSPlatform),
                typeof(TorSharpPrivoxySettings),
                typeof(TorSharpProxy),
                typeof(TorSharpSettings),
                typeof(TorSharpToolFetcher),
                typeof(TorSharpTorSettings),
            };

            var publicTypes = typeof(TorSharpProxy)
                .Assembly
                .GetTypes()
                .Where(x => x.IsPublic)
                .ToList();

            Assert.Empty(publicTypes.Except(expectedTypes));
            Assert.Empty(expectedTypes.Except(publicTypes));
        }
    }
}
