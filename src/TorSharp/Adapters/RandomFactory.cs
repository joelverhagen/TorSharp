using System.Security.Cryptography;

namespace Knapcode.TorSharp.Adapters
{
    internal class RandomFactory : IRandomFactory
    {
        public IRandom Create()
        {
            return new Random(new RNGCryptoServiceProvider());
        }
    }
}