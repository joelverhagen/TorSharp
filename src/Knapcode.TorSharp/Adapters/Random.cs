using System;
using System.Security.Cryptography;

namespace Knapcode.TorSharp.Adapters
{
    public class Random : IRandom
    {
        private readonly RandomNumberGenerator _rng;

        public Random(RandomNumberGenerator rng)
        {
            _rng = rng;
        }

        public void Dispose()
        {
            _rng.Dispose();
        }

        public void GetBytes(byte[] bytes)
        {
            _rng.GetBytes(bytes);
        }
    }

    public class RandomFactory : IRandomFactory
    {
        public IRandom Create()
        {
            return new Random(new RNGCryptoServiceProvider());
        }
    }

    public interface IRandomFactory
    {
        IRandom Create();
    }

    public interface IRandom : IDisposable
    {
        void GetBytes(byte[] bytes);
    }
}