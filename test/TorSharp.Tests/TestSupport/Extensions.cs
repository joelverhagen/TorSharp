using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Xunit.Abstractions;

namespace Knapcode.TorSharp.Tests.TestSupport
{
    public static class Extensions
    {
        public static void WriteLine(this ITestOutputHelper output, TorSharpSettings settings)
        {
            var serializerSettings = new JsonSerializerSettings
            {
                Converters =
                {
                    new StringEnumConverter()
                }
            };

            var json = JsonConvert.SerializeObject(settings, Formatting.Indented, serializerSettings);

            output.WriteLine($"{nameof(TorSharpSettings)}:" + Environment.NewLine + json);
        }
    }
}