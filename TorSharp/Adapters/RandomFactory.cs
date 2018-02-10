using System.Security.Cryptography;

namespace Knapcode.TorSharp.Adapters
{
    public class RandomFactory : IRandomFactory
    {
        public IRandom Create()
        {
            return new Random(new RNGCryptoServiceProvider());
        }
    }
}