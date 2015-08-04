using System;
using System.Runtime.Serialization;

namespace Knapcode.NetTor.Tools.Tor
{
    [Serializable]
    public class TorControlException : NetTorException
    {
        public TorControlException(string message) : base(message)
        {
        }

        protected TorControlException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}