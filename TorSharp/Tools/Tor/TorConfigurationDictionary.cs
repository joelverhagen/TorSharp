using System;
using System.Collections.Generic;
using System.Globalization;

namespace Knapcode.TorSharp.Tools.Tor
{
    public class TorConfigurationDictionary : IConfigurationDictionary
    {
        public IDictionary<string, string> GetDictionary(TorSharpSettings settings)
        {
            var dictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "SocksPort", settings.TorSocksPort.ToString(CultureInfo.InvariantCulture) },
                { "ControlPort", settings.TorControlPort.ToString(CultureInfo.InvariantCulture) }
            };

            if (settings.HashedTorControlPassword != null)
            {
                dictionary["HashedControlPassword"] = settings.HashedTorControlPassword;
            }

            if (!string.IsNullOrWhiteSpace(settings.TorDataDirectory))
            {
                dictionary["DataDirectory"] = settings.TorDataDirectory;
            }

            if (settings.TorStrictNodes != null)
            {
                dictionary["StrictNodes"] = (bool)settings.TorStrictNodes ? "1" : "0";
            }

            return dictionary;
        }
    }
}