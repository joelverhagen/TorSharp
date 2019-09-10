using Xunit;

namespace Knapcode.TorSharp.Tests.TestSupport
{
    [CollectionDefinition(Name)]
    public class HttpCollection : ICollectionFixture<HttpFixture>
    {
        public const string Name = nameof(HttpCollection);
    }
}
