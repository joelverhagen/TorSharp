using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Knapcode.TorSharp.Tools
{
    internal class LineByLineConfigurer
    {
        private readonly IConfigurationDictionary _configurationDictionary;
        private readonly IConfigurationFormat _format;

        public LineByLineConfigurer(IConfigurationDictionary configurationDictionary, IConfigurationFormat format)
        {
            _configurationDictionary = configurationDictionary;
            _format = format;
        }

        public async Task ApplySettings(Tool tool, TorSharpSettings settings)
        {
            // convert the settings to a dictionary
            var dictionary = _configurationDictionary.GetDictionary(tool, settings);
            
            // write the new settings
            string temporaryPath = null;
            try
            {
                // write first to a temporary file
                temporaryPath = Path.GetTempFileName();
                TextReader reader;

                // read the existing configuration, if there is some
                if (File.Exists(tool.ConfigurationPath))
                {
                    reader = new StreamReader(new FileStream(tool.ConfigurationPath, FileMode.Open, FileAccess.Read, FileShare.None));
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
                    while ((originalLine = await reader.ReadLineAsync().ConfigureAwait(false)) != null)
                    {
                        string newLine = dictionary.Count > 0 ? _format.UpdateLine(dictionary, originalLine) : originalLine;
                        if (newLine != null)
                        {
                            await writer.WriteLineAsync(newLine).ConfigureAwait(false);
                        }
                    }

                    // write the remaining lines
                    foreach (var pair in dictionary.OrderBy(p => p.Key))
                    {
                        if (pair.Value != null && pair.Value.Any())
                        {
                            foreach (var value in pair.Value)
                            {
                                string newLine = _format.CreateLine(new KeyValuePair<string, string>(pair.Key, value));
                                await writer.WriteLineAsync(newLine).ConfigureAwait(false);
                            }
                        }
                    }
                }

                // If the original file exists, remove it before copying over the temporary file.
                if (File.Exists(tool.ConfigurationPath))
                {
                    string backupPath = tool.ConfigurationPath + ".bak";

                    // If there has already been a backup, delete it.
                    if (File.Exists(backupPath))
                    {
                        File.Delete(backupPath);
                    }

                    // Backup the last config.
                    File.Move(tool.ConfigurationPath, backupPath);
                }

                File.Move(temporaryPath, tool.ConfigurationPath);
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
