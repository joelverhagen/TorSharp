using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Knapcode.TorSharp.Tools
{
    internal interface IConfigurationFormat
    {
        string UpdateLine(IDictionary<string, List<string>> dictionary, string originalLine);
        string CreateLine(KeyValuePair<string, string> pair);
    }

    internal class ConfigurationFormat : IConfigurationFormat
    {
        public Regex CommentPattern => new Regex(@"^\s*#+");

        public string UpdateLine(IDictionary<string, List<string>> dictionary, string originalLine)
        {
            // try to match the key
            var pieces = Regex.Split(originalLine.Trim(), @"\s+");
            if (pieces.Length < 2)
            {
                return originalLine;
            }

            string key = pieces[0];
            if (dictionary.TryGetValue(key, out var values))
            {
                if (values == null || values.Count == 0)
                {
                    dictionary.Remove(key);
                }
                else
                {
                    var keyMatch = Regex.Match(originalLine, @"^(\s*)(?<Key>" + Regex.Escape(key) + @")(\s+)");
                    var value = values[0];

                    values.RemoveAt(0);
                    if (values.Count == 0)
                    {
                        dictionary.Remove(key);
                    }

                    if (value == null)
                    {
                        return null;
                    }
                    else
                    {
                        return CreateLine(new KeyValuePair<string, string>(keyMatch.Groups["Key"].Value, value));
                    }
                }
            }

            return originalLine;
        }

        public string CreateLine(KeyValuePair<string, string> pair)
        {
            return $"{pair.Key} {pair.Value}";
        }
    }
}