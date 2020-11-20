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

            if (settings.TorSettings.UseBridges.HasValue)
            {
                dictionary["UseBridges"] = settings.TorSettings.UseBridges.Value ? "1" : "0";
            }

            if (settings.TorSettings.BridgeRelay.HasValue)
            {
                dictionary["BridgeRelay"] = settings.TorSettings.BridgeRelay.Value ? "1" : "0";
            }

            if (!string.IsNullOrWhiteSpace(settings.TorSettings.Nickname))
            {
                dictionary["Nickname"] = settings.TorSettings.Nickname;
            }

            if (!string.IsNullOrWhiteSpace(settings.TorSettings.ContactInfo))
            {
                dictionary["ContactInfo"] = settings.TorSettings.ContactInfo;
            }

            if (!string.IsNullOrWhiteSpace(settings.TorSettings.ServerTransportListenAddr))
            {
                dictionary["ServerTransportListenAddr"] = settings.TorSettings.ServerTransportListenAddr;
            }

            if (!string.IsNullOrWhiteSpace(settings.TorSettings.ExitPolicy))
            {
                dictionary["ExitPolicy"] = settings.TorSettings.ExitPolicy;
            }

            if (!string.IsNullOrWhiteSpace(settings.TorSettings.PublishServerDescriptor))
            {
                dictionary["PublishServerDescriptor"] = settings.TorSettings.PublishServerDescriptor;
            }

            if (!string.IsNullOrWhiteSpace(settings.TorSettings.ExtORPort))
            {
                dictionary["ExtORPort"] = settings.TorSettings.ExtORPort;
            }

            if (!string.IsNullOrWhiteSpace(settings.TorSettings.ORPort))
            {
                dictionary["ORPort"] = settings.TorSettings.ORPort;
            }

            if (!string.IsNullOrWhiteSpace(settings.TorSettings.ClientTransportPlugin))
            {
                dictionary["ClientTransportPlugin"] = settings.TorSettings.ClientTransportPlugin;
            }

            if (!string.IsNullOrWhiteSpace(settings.TorSettings.Bridge))
            {
                dictionary["Bridge"] = settings.TorSettings.Bridge;
            }

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
