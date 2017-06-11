using System;
using System.Net;

namespace Knapcode.TorSharp
{
    public class SimpleWebProxy : IWebProxy
    {
        private readonly Uri _uri;

        public SimpleWebProxy(Uri uri)
        {
            _uri = uri;
        }

        public ICredentials Credentials { get; set; }

        public Uri GetProxy(Uri destination)
        {
            return _uri;
        }

        public bool IsBypassed(Uri host)
        {
            return false;
        }
    }
}
