using System;
using System.Runtime.Serialization;

namespace Knapcode.TorSharp.Tools.Tor
{
    [Serializable]
    public class TorControlException : TorSharpException
    {
        public TorControlException(string message) : base(message)
        {
        }

        protected TorControlException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}