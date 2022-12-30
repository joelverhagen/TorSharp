using xRetry;

namespace Knapcode.TorSharp.Tests.TestSupport
{
    public class PlatformTheory: RetryTheoryAttribute
    {
        public PlatformTheory(string osPlatform = null, string architecture = null)
        {
            var settings = new TorSharpSettings();

            var currentOSPlatform = settings.OSPlatform.ToString();
            if (osPlatform != null && osPlatform != currentOSPlatform)
            {
                Skip = $"This test is not run on platform '{currentOSPlatform}'.";
            }

            var currentArchitecture = settings.Architecture.ToString();
            if (architecture != null && architecture != currentArchitecture)
            {
                Skip = $"This test is not run on architecture '{currentArchitecture}'.";
            }
        }
    }
}