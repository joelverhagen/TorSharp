using System.Collections.Generic;

namespace Knapcode.NetTor.Tools
{
    public interface IConfigurationDictionary
    {
        IDictionary<string, string> GetDictionary(NetTorSettings settings);
    }
}