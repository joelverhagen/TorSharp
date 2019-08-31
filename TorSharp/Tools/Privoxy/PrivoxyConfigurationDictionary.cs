using System;
using System.Collections.Generic;
using System.IO;

namespace Knapcode.TorSharp.Tools.Privoxy
{
    public class PrivoxyConfigurationDictionary : IConfigurationDictionary
    {
        public IDictionary<string, string> GetDictionary(Tool tool, TorSharpSettings settings)
        {
            var output = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                {"listen-address", $"{settings.PrivoxySettings.ListenAddress}:{settings.PrivoxySettings.Port}"},
                {"forward-socks5t", $"/ 127.0.0.1:{settings.TorSettings.SocksPort} ."},
            };

            if (settings.OSPlatform == TorSharpOSPlatform.Windows)
            {
                output["close-button-minimizes"] = "1";
                output["show-on-task-bar"] = "0";
                output["activity-animation"] = "0";
            }

            return output;
        }
    }
}