using System.Collections.Generic;

namespace Knapcode.TorSharp.Tools
{
    public interface IConfigurationFormat
    {
        string UpdateLine(IDictionary<string, string> dictionary, string originalLine);
        string CreateLine(KeyValuePair<string, string> pair);
    }
}