using System;

namespace Knapcode.TorSharp.Tools
{
    internal static class EnumHelper
    {
        public static bool Contains<T>(this T value, string enumValue) where T : Enum
        {
            return value.ToString().Contains(enumValue);
        }
    }
}
