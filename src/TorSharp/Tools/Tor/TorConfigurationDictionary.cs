using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace Knapcode.TorSharp.Tools.Tor
{
    internal class TorConfigurationDictionary : IConfigurationDictionary
    {
        public IDictionary<string, string> GetDictionary(Tool tool, TorSharpSettings settings)
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
            else
            {
                dictionary["HashedControlPassword"] = null;
            }

            string dataDictionary;
            if (!string.IsNullOrWhiteSpace(settings.TorSettings.DataDirectory))
            {
                dataDictionary = settings.TorSettings.DataDirectory;
            }
            else
            {
                dataDictionary = Path.Combine(tool.DirectoryPath, "Data", "Tor");
            }

            dictionary["DataDirectory"] = dataDictionary;

            if (!string.IsNullOrWhiteSpace(settings.TorSettings.ExitNodes))
            {
                dictionary["ExitNodes"] = settings.TorSettings.ExitNodes;
                dictionary["GeoIPFile"] = Path.Combine(tool.DirectoryPath, "Data", "Tor", "geoip");
                dictionary["GeoIPv6File"] = Path.Combine(tool.DirectoryPath, "Data", "Tor", "geoip6");
            }

            if (settings.TorSettings.StrictNodes.HasValue)
            {
                dictionary["StrictNodes"] = settings.TorSettings.StrictNodes.Value ? "1" : "0";
            }

            if (!string.IsNullOrWhiteSpace(settings.TorSettings.HttpsProxyHost))
            {
                dictionary["HTTPSProxy"] = settings.TorSettings.HttpsProxyHost;

                if (settings.TorSettings.HttpsProxyPort.HasValue)
                {
                    dictionary["HTTPSProxy"] +=
                        ":" + settings.TorSettings.HttpsProxyPort.Value.ToString(CultureInfo.InvariantCulture);
                }
            }

            if (settings.TorSettings.HttpsProxyUsername != null
                || settings.TorSettings.HttpsProxyPassword != null)
            {
                dictionary["HTTPSProxyAuthenticator"] =
                    $"{settings.TorSettings.HttpsProxyUsername}:{settings.TorSettings.HttpsProxyPassword}";
            }

            return dictionary;
        }
    }
}
