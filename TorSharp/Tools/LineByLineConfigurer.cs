using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Knapcode.TorSharp.Tools
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

        public async Task ApplySettings(string path, TorSharpSettings settings)
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
                    while ((originalLine = await reader.ReadLineAsync().ConfigureAwait(false)) != null)
                    {
                        string newLine = dictionary.Count > 0 ? _format.UpdateLine(dictionary, originalLine) : originalLine;
                        await writer.WriteLineAsync(newLine).ConfigureAwait(false);
                    }

                    // write the remaining lines
                    foreach (var pair in dictionary.OrderBy(p => p.Key))
                    {
                        string newLine = _format.CreateLine(pair);
                        await writer.WriteLineAsync(newLine).ConfigureAwait(false);
                    }
                }

                // If the original file exists, remove it before copying over the temporary file.
                if (File.Exists(path))
                {
                    string backupPath = path + ".bak";

                    // If there has already been a backup, just delete the original.
                    if (File.Exists(backupPath))
                    {
                        File.Delete(path);
                    }
                    else
                    {
                        // Backup the original if there is not already a backup.
                        File.Move(path, backupPath);
                    }
                }

                File.Move(temporaryPath, path);
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
