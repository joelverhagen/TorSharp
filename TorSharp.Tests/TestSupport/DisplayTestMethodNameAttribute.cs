using System;
using System.Reflection;
using Xunit.Sdk;

namespace Knapcode.TorSharp.Tests.TestSupport
{
    /// <summary>
    /// Source: https://stackoverflow.com/a/26042654
    /// </summary>
    public class DisplayTestMethodNameAttribute : BeforeAfterTestAttribute
    {
        public override void Before(MethodInfo methodUnderTest)
        {
            // Console.WriteLine($"🌟 Starting {methodUnderTest.Name}");
        }

        public override void After(MethodInfo methodUnderTest)
        {
            // Console.WriteLine($"🛑 Finished {methodUnderTest.Name}");
        }
    }
}