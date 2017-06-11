using System;

namespace Knapcode.TorSharp
{
    public class TorSharpException : Exception
    {
        public TorSharpException(string message) : base(message)
        {
        }
    }
}