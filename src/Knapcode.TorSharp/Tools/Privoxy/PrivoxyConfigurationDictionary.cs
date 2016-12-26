using System;
using System.Collections.Generic;

namespace Knapcode.TorSharp.Tools.Privoxy
{
    public class PrivoxyConfigurationDictionary : IConfigurationDictionary
    {
        public IDictionary<string, string> GetDictionary(TorSharpSettings settings)
        {
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                {"listen-address", $"127.0.0.1:{settings.PrivoxyPort}"},
                {"forward-socks5t", $"/ 127.0.0.1:{settings.TorSocksPort} ."},
                {"close-button-minimizes", "1"},
                {"show-on-task-bar", "0"},
                {"activity-animation", "0"}
            };
        }
    }
}