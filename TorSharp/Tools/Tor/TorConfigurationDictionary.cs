using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace Knapcode.TorSharp.Tools.Tor
{
    public class TorConfigurationDictionary : IConfigurationDictionary
    {
        private readonly string _torDirectoryPath;

        public TorConfigurationDictionary(string torDirectoryPath)
        {
            _torDirectoryPath = torDirectoryPath;
        }

        public IDictionary<string, string> GetDictionary(TorSharpSettings settings)
        {
            var dictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "SocksPort", settings.TorSettings.SocksPort.ToString(CultureInfo.InvariantCulture) },
                { "ControlPort", settings.TorSettings.ControlPort.ToString(CultureInfo.InvariantCulture) }
            };

            if (settings.TorSettings.HashedControlPassword != null)
            {
                dictionary["HashedControlPassword"] = settings.TorSettings.HashedControlPassword;
            }

            string dataDictionary;
            if (!string.IsNullOrWhiteSpace(settings.TorSettings.DataDirectory))
            {
                dataDictionary = settings.TorSettings.DataDirectory;
            }
            else
            {
                dataDictionary = Path.Combine(_torDirectoryPath, "Data", "Tor");
            }

            dictionary["DataDirectory"] = dataDictionary;

            if (!string.IsNullOrWhiteSpace(settings.TorSettings.ExitNodes))
            {
                dictionary["ExitNodes"] = settings.TorSettings.ExitNodes;
                dictionary["GeoIPFile"] = Path.Combine(_torDirectoryPath, "Data", "Tor", "geoip");
                dictionary["GeoIPv6File"] = Path.Combine(_torDirectoryPath, "Data", "Tor", "geoip6");
            }

            if (settings.TorSettings.StrictNodes.HasValue)
            {
                dictionary["StrictNodes"] = settings.TorSettings.StrictNodes.Value ? "1" : "0";
            }

            return dictionary;
        }
    }
}
