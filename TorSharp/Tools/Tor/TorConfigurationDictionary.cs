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
                { "SocksPort", settings.TorSocksPort.ToString(CultureInfo.InvariantCulture) },
                { "ControlPort", settings.TorControlPort.ToString(CultureInfo.InvariantCulture) }
            };

            if (settings.HashedTorControlPassword != null)
            {
                dictionary["HashedControlPassword"] = settings.HashedTorControlPassword;
            }

            dictionary["DataDirectory"] = !string.IsNullOrWhiteSpace(settings.TorDataDirectory) ? settings.TorDataDirectory : Path.Combine(_torDirectoryPath, "Data\\Tor");

            if (settings.TorExitNodes != null)
            {
                dictionary["ExitNodes"] = settings.TorExitNodes;
                dictionary["GeoIPFile"] = Path.Combine(_torDirectoryPath, Path.Combine("Data", "Tor", "geoip"));
                dictionary["GeoIPv6File"] = Path.Combine(_torDirectoryPath, Path.Combine("Data", "Tor", "geoip6"));
            }

            if (settings.TorStrictNodes != null)
            {
                dictionary["StrictNodes"] = settings.TorStrictNodes.Value ? "1" : "0";
            }

            return dictionary;
        }
    }
}
