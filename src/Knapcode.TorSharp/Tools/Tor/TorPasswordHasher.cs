using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Knapcode.TorSharp.Adapters;

namespace Knapcode.TorSharp.Tools.Tor
{
    public class TorPasswordHasher
    {
        private readonly IRandomFactory _randomFactory;

        public TorPasswordHasher(IRandomFactory randomFactory)
        {
            _randomFactory = randomFactory;
        }

        public string HashPassword(string password)
        {
            // Implement the Tor password hashing algorithm. Source:
            // https://gitweb.torproject.org/tor.git/tree/src/or/main.c?id=1f679d4ae11cd976f5539bc4ddf36873132aeb00#n3217
            // https://gitweb.torproject.org/tor.git/tree/src/common/crypto_s2k.c?id=1f679d4ae11cd976f5539bc4ddf36873132aeb00#n172
            byte[] salt = new byte[8];
            using (var random = _randomFactory.Create())
            {
                random.GetBytes(salt);
            }

            var c = 96;
            var s2KSpecifier = salt.Concat(new[] {(byte) c}).ToArray();

            var EXPBIAS = 6;
            var count = (16 + (c & 15)) << ((c >> 4) + EXPBIAS);

            byte[] hash;
            using (var d = SHA1.Create())
            {
                var tmp = s2KSpecifier
                       .Take(8)
                       .Concat(Encoding.ASCII.GetBytes(password))
                       .ToArray();

                var secretLen = tmp.Length;
                while (count != 0)
                {
                    if (count > secretLen)
                    {
                        d.TransformBlock(tmp, 0, tmp.Length, null, -1);
                        count -= secretLen;
                    }
                    else
                    {
                        d.TransformBlock(tmp, 0, count, null, -1);
                        count = 0;
                    }
                }

                d.TransformFinalBlock(new byte[0], 0, 0);
                hash = d.Hash;
            }

            var s2KSpecifierHex = BytesToHex(s2KSpecifier);
            var hashHex = BytesToHex(hash);

            return $"16:{s2KSpecifierHex}{hashHex}";
        }

        private static string BytesToHex(byte[] bytes)
        {
            return BitConverter
                .ToString(bytes, 0, bytes.Length)
                .Replace("-", string.Empty)
                .ToUpper();
        }
    }
}
