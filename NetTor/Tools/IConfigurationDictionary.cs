using System.Collections.Generic;

namespace Knapcode.TorSharp.Tools
{
    public interface IConfigurationDictionary
    {
        IDictionary<string, string> GetDictionary(TorSharpSettings settings);
    }
}