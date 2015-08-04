using System;
using System.Runtime.Serialization;

namespace Knapcode.NetTor
{
    [Serializable]
    public class NetTorException : Exception
    {
        public NetTorException(string message) : base(message)
        {
        }

        protected NetTorException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}