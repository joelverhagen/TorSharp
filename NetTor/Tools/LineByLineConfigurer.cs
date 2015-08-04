using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Knapcode.NetTor.Tools
{
    public class LineByLineConfigurer
    {
        private readonly IConfigurationDictionary _configurationDictionary;
        private readonly IConfigurationFormat _format;

        public LineByLineConfigurer(IConfigurationDictionary configurationDictionary, IConfigurationFormat format)
        {
            _configurationDictionary = configurationDictionary;
            _format = format;
        }

        public async Task ApplySettings(string path, NetTorSettings settings)
        {
            // convert the settings to a dictionary
            var dictionary = _configurationDictionary.GetDictionary(settings);
            
            // write the new settings
            string temporaryPath = null;
            try
            {
                // write first to a temporary file
                temporaryPath = Path.GetTempFileName();
                TextReader reader;

                // read the existing configuration, if there is some
                if (File.Exists(path))
                {
                    reader = new StreamReader(new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.None));
                }
                else
                {
                    reader = new StringReader(string.Empty);
                }

                using (reader)
                using (var writer = new StreamWriter(new FileStream(temporaryPath, FileMode.Create, FileAccess.Write, FileShare.None)))
                {
                    // traverse each line looking for the designed configuration
                    string originalLine;
                    while ((originalLine = await reader.ReadLineAsync()) != null)
                    {
                        string newLine = dictionary.Count > 0 ? _format.UpdateLine(dictionary, originalLine) : originalLine;
                        await writer.WriteLineAsync(newLine);
                    }

                    // write the remaining lines
                    foreach (var pair in dictionary.OrderBy(p => p.Key))
                    {
                        string newLine = _format.CreateLine(pair);
                        await writer.WriteLineAsync(newLine);
                    }
                }

                if (File.Exists(path))
                {
                    File.Replace(temporaryPath, path, null);
                }
                else
                {
                    File.Move(temporaryPath, path);
                }
            }
            finally
            {
                if (temporaryPath != null)
                {
                    try
                    {
                        File.Delete(temporaryPath);
                    }
                    catch
                    {
                        // ignored
                    }
                }
            }

        }
    }
}
