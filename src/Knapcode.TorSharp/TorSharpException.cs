using System;
using System.Runtime.Serialization;

namespace Knapcode.TorSharp
{
    [Serializable]
    public class TorSharpException : Exception
    {
        public TorSharpException(string message) : base(message)
        {
        }

        protected TorSharpException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}