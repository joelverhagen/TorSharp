using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Knapcode.TorSharp.Tests.TestSupport;
using xRetry;
using Xunit;
using Xunit.Abstractions;

namespace Knapcode.TorSharp.Tests
{
    public class TestAppTests : IClassFixture<TestAppFixture>
    {
        private readonly TestAppFixture _fixture;
        private readonly ITestOutputHelper _output;

        public TestAppTests(TestAppFixture fixture, ITestOutputHelper output)
        {
            _fixture = fixture;
            _output = output;

            ArtifactsDir = Path.Combine(_fixture.SharedDirectory, "artifacts");
            ZippedDir = Path.Combine(_fixture.SharedDirectory, "zipped");
            ExtractedDir = Path.Combine(_fixture.SharedDirectory, "extracted");
            ProjectDir = GetTestAppProjectDir();
        }

        private string ArtifactsDir { get; }
        private string ZippedDir { get; }
        private string ExtractedDir { get; }
        private string ProjectDir { get; }

        [PlatformTheory(osPlatform: nameof(TorSharpOSPlatform.Windows))]
        [InlineData(false, "net462")]
        [InlineData(true, "net462")]
        [InlineData(false, "net472")]
        [InlineData(true, "net472")]
        [InlineData(false, "net6.0")]
        [InlineData(true, "net6.0")]
        [DisplayTestMethodName]
        public void VirtualDesktopToolRunner_OnlyWritesToStdoutIfSpecified(bool writeToConsole, string framework)
        {
            Execute(writeToConsole, ToolRunnerType.VirtualDesktop, framework);
        }

        [RetryTheory]
        [InlineData(false, "net6.0")]
        [InlineData(true, "net6.0")]
        [DisplayTestMethodName]
        public void SimpleToolRuner_OnlyWritesToStdoutIfSpecified(bool writeToConsole, string framework)
        {
            Execute(writeToConsole, ToolRunnerType.Simple, framework);
        }

        private void Execute(bool writeToConsole, ToolRunnerType toolRunnerType, string framework)
        {
            // build the test app
            ExecuteDotnet(
                new[]
                {
                    "build", "\"" + ProjectDir + "\"",
                    "--configuration", "Debug",
                    "--framework", framework
                },
                ProjectDir,
                env: new Dictionary<string, string>
                {
                    { "ArtifactsDirectory", ArtifactsDir },
                },
                out _);

            // Act
            // run the test app
            ExecuteDotnet(
                new[]
                {
                    "run", "--project", "\"" + ProjectDir + "\"",
                    "--configuration", "Debug",
                    "--framework", framework,
                    "--no-restore", "--no-build",
                    "--",
                    writeToConsole.ToString(),
                    toolRunnerType.ToString(),
                    "\"" + ZippedDir + "\"",
                    "\"" + ExtractedDir + "\"",
                },
                ProjectDir,
                new Dictionary<string, string>
                {
                    { "ArtifactsDirectory", ArtifactsDir },
                },
                out var output);

            // Assert
            if (writeToConsole)
            {
                // This is a line that Tor will write
                Assert.Contains(output, x => x.Contains("Opening Socks listener on 127.0.0.1"));

                // This is a line the test app will write
                Assert.Contains(output, x => x.Contains("TestApp says it's done!"));
            }
            else
            {
                // This is a line the test app will write
                Assert.Equal(new[] { "TestApp says it's done!" }, output.ToArray());
            }
        }

        private static string GetTestAppProjectDir()
        {
            var current = Directory.GetCurrentDirectory();
            while (current != null && !Directory.EnumerateFiles(current, "TorSharp.sln").Any())
            {
                current = Path.GetDirectoryName(current);
            }

            if (current == null)
            {
                throw new InvalidOperationException($"Could not find the repository root by probing upwards for TorSharp.sln. Current directory: {Directory.GetCurrentDirectory()}");
            }

            return Path.Combine(current, "test", "TestApp");
        }

        private void ExecuteDotnet(string[] args, string workingDir, IReadOnlyDictionary<string, string> env, out ConcurrentQueue<string> output)
        {
            var startInfo = new ProcessStartInfo("dotnet", string.Join(" ", args));
            startInfo.WorkingDirectory = workingDir;
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            foreach (var pair in env)
            {
                startInfo.Environment[pair.Key] = pair.Value;
            }

            _output.WriteLine("Starting: dotnet " + startInfo.Arguments);
            var process = Process.Start(startInfo);

            output = new ConcurrentQueue<string>();

            var outputReference = output;

            process.OutputDataReceived += (_, e) =>
            {
                _output.WriteLine("[stdout] " + (e.Data ?? "(null)"));
                if (e.Data != null)
                {
                    outputReference.Enqueue(e.Data);
                }
            };
            process.ErrorDataReceived += (_, e) =>
            {
                _output.WriteLine("[stderr] " + (e.Data ?? "(null)"));
                if (e.Data != null)
                {
                    outputReference.Enqueue(e.Data);
                }
            };

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            var maxWait = TimeSpan.FromMinutes(3);
            var exited = process.WaitForExit((int)maxWait.TotalMilliseconds);
            if (!exited)
            {
                process.Kill();
            }

            Assert.True(exited, $"The command '{startInfo.FileName} {startInfo.Arguments}' did not exit after waiting {maxWait}.");
            Assert.Equal(0, process.ExitCode);
        }
    }
}
