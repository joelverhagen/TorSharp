#if NETFRAMEWORK
using System;
using System.Threading.Tasks;

namespace Knapcode.TorSharp.Tools
{
    internal partial class ArchiveUtility
    {
        public static Task TestDebAsync(string debPath)
        {
            throw new NotImplementedException(
                $"Testing {GetFileExtension(ZippedToolFormat.Deb)} files .NET Framework is not supported.");
        }

        public static Task ExtractDebAsync(
            string debPath,
            string outputDir,
            Func<string, string> getEntryPath)
        {
            throw new NotImplementedException(
                $"Extracting {GetFileExtension(ZippedToolFormat.Deb)} files .NET Framework is not supported.");
        }

        public static Task TestTarXzAsync(string tarXzPath)
        {
            throw new NotImplementedException(
                $"Testing {GetFileExtension(ZippedToolFormat.TarXz)} files .NET Framework is not supported.");
        }

        public static Task ExtractTarXzAsync(
            string tarXzPath,
            string outputDir,
            Func<string, string> getEntryPath)
        {
            throw new NotImplementedException(
                $"Extracting {GetFileExtension(ZippedToolFormat.TarXz)} files .NET Framework is not supported.");
        }
    }
}
#endif
