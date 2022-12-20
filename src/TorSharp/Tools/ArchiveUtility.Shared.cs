using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

namespace Knapcode.TorSharp.Tools
{
    internal partial class ArchiveUtility
    {
        public static async Task TestAsync(ZippedToolFormat format, string path)
        {
            switch (format)
            {
                case ZippedToolFormat.Zip:
                    await TestZipAsync(path).ConfigureAwait(false);
                    break;
                case ZippedToolFormat.Deb:
                    await TestDebAsync(path).ConfigureAwait(false);
                    break;
                case ZippedToolFormat.TarXz:
                    await TestTarXzAsync(path).ConfigureAwait(false);
                    break;
                case ZippedToolFormat.TarGz:
                    await TestTarGzAsync(path).ConfigureAwait(false);
                    break;
                default:
                    throw new NotImplementedException(
                        $"Testing the zipped tool format {format} is not supported.");
            }
        }

        public static async Task ExtractAsync(
            ZippedToolFormat format,
            string path,
            string outputDir,
            Func<string, string> getEntryPath)
        {
            switch (format)
            {
                case ZippedToolFormat.Zip:
                    await ExtractZipAsync(path, outputDir, getEntryPath).ConfigureAwait(false);
                    break;
                case ZippedToolFormat.Deb:
                    await ExtractDebAsync(path, outputDir, getEntryPath).ConfigureAwait(false);
                    break;
                case ZippedToolFormat.TarXz:
                    await ExtractTarXzAsync(path, outputDir, getEntryPath).ConfigureAwait(false);
                    break;
                case ZippedToolFormat.TarGz:
                    await ExtractTarGzAsync(path, outputDir, getEntryPath).ConfigureAwait(false);
                    break;
                default:
                    throw new NotImplementedException(
                        $"Extracting the zipped tool format {format} is not supported.");
            }
        }

        public static string GetFileExtension(ZippedToolFormat format)
        {
            switch (format)
            {
                case ZippedToolFormat.Zip:
                    return ".zip";
                case ZippedToolFormat.Deb:
                    return ".deb";
                case ZippedToolFormat.TarXz:
                    return ".tar.xz";
                case ZippedToolFormat.TarGz:
                    return ".tar.gz";
                default:
                    throw new NotImplementedException($"The zipped tool format {format} does not have a known extension.");
            }
        }

        public static async Task TestZipAsync(string zipPath)
        {
            await ReadZipAsync(
                zipPath,
                outputDir: null,
                getEntryPath: null,
                shouldExtract: false).ConfigureAwait(false);
        }

        public static async Task ExtractZipAsync(
            string zipPath,
            string outputDir,
            Func<string, string> getEntryPath)
        {
            await ReadZipAsync(
                zipPath,
                outputDir,
                getEntryPath,
                shouldExtract: true).ConfigureAwait(false);
        }

        private static async Task ReadZipAsync(
            string zipPath,
            string outputDir,
            Func<string, string> getEntryPath,
            bool shouldExtract)
        {
            using (var fileStream = new FileStream(zipPath, FileMode.Open))
            using (var zipArchive = new ZipArchive(fileStream, ZipArchiveMode.Read))
            {
                var entries = zipArchive.Entries;

                if (!shouldExtract)
                {
                    return;
                }

                var createdDirs = new HashSet<string>();
                foreach (var entry in entries)
                {
                    if (entry.FullName.EndsWith("/"))
                    {
                        continue;
                    }

                    var entryPath = getEntryPath(entry.FullName);
                    if (entryPath == null)
                    {
                        continue;
                    }

                    var fullEntryPath = Path.GetFullPath(Path.Combine(outputDir, entryPath));
                    var entryDir = Path.GetDirectoryName(fullEntryPath);
                    if (createdDirs.Add(entryDir))
                    {
                        Directory.CreateDirectory(entryDir);
                    }

                    using (var entryStream = entry.Open())
                    using (var outputStream = new FileStream(fullEntryPath, FileMode.Create))
                    {
                        await entryStream.CopyToAsync(outputStream).ConfigureAwait(false);
                    }
                }
            }
        }
    }
}
