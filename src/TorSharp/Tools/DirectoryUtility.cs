using System.IO;

namespace Knapcode.TorSharp.Tools
{
    internal static class DirectoryUtility
    {
        public static void CreateDirectoryIfNotExists(params string[] path)
        {
            foreach (var p in path)
                CreateDirectoryIfNotExists(p);
        }

        public static void CreateDirectoryIfNotExists(string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }
    }
}
