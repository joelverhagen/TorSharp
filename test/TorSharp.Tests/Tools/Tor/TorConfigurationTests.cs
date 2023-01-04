using System;
using System.IO;
using System.Threading.Tasks;
using Knapcode.TorSharp.Tests.TestSupport;
using Knapcode.TorSharp.Tools;
using Knapcode.TorSharp.Tools.Tor;
using Xunit;
using Xunit.Abstractions;

namespace Knapcode.TorSharp.Tests.Tools.Tor
{
    public class TorConfigurationTests : IDisposable
    {
        public TorConfigurationTests(ITestOutputHelper output)
        {
            Environment = TestEnvironment.Initialize(output);
            Tool = new Tool
            {
                DirectoryPath = Environment.TestDirectory,
                ConfigurationPath = Path.Combine(Environment.TestDirectory, "torrc"),
            };
            Target = new TorConfigurationDictionary();
        }

        public TestEnvironment Environment { get; }
        internal Tool Tool { get; }
        internal TorConfigurationDictionary Target { get; }

        public void Dispose()
        {
            Environment.Dispose();
        }

        [Fact]
        public async Task HashedControlPasswordGetsCleared()
        {
            // Arrange
            var format = new ConfigurationFormat();
            var configurer = new LineByLineConfigurer(Target, format);
            var settings = Environment.BuildSettings();
            settings.TorSettings.HashedControlPassword = "NOT_A_REAL_HASH";
            await configurer.ApplySettings(Tool, settings);
            var configurationBefore = File.ReadAllText(Tool.ConfigurationPath);

            // Act
            settings.TorSettings.HashedControlPassword = null;
            await configurer.ApplySettings(Tool, settings);

            // Assert
            var configurationAfter = File.ReadAllText(Tool.ConfigurationPath);
            Assert.Contains("HashedControlPassword NOT_A_REAL_HASH", configurationBefore);
            Assert.DoesNotContain("HashedControlPassword", configurationAfter);
        }

        [Fact]
        public async Task DoesNotAddEmptyHashedControlPassword()
        {
            // Arrange
            var format = new ConfigurationFormat();
            var configurer = new LineByLineConfigurer(Target, format);
            var settings = Environment.BuildSettings();
            settings.TorSettings.HashedControlPassword = null;

            // Act
            await configurer.ApplySettings(Tool, settings);

            // Assert
            var configurationAfter = File.ReadAllText(Tool.ConfigurationPath);
            Assert.DoesNotContain("HashedControlPassword", configurationAfter);
        }
    }
}
