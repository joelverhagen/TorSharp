using System;
using System.IO;
using System.Linq;

namespace Knapcode.TorSharp.Tools
{
    public static class SearchInPathHelper
    {
        public static string SearchInPathVariable(string pattern)
        {
            var variables = Environment.GetEnvironmentVariables();
            if (!variables.Contains("Path"))
                return null;

            var what = variables["Path"].ToString()
                .Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                .SelectMany(a => Directory.GetFiles(a, pattern, SearchOption.TopDirectoryOnly));

            return what.FirstOrDefault();
        }
    }
}
