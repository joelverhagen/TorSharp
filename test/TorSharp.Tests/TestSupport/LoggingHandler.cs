using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Knapcode.TorSharp.Tests.TestSupport
{
    public class LoggingHandler : DelegatingHandler
    {
        private readonly ITestOutputHelper _output;
        private static int _requestCounter;

        public LoggingHandler(ITestOutputHelper output)
        {
            _output = output;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var requestId = Interlocked.Increment(ref _requestCounter);
            _output.WriteLine($"[{DateTimeOffset.UtcNow:O}] [{requestId}] {request.Method} {request.RequestUri.AbsoluteUri}");
            try
            {
                var response = await base.SendAsync(request, cancellationToken);
                _output.WriteLine($"[{DateTimeOffset.UtcNow:O}] [{requestId}] {(int)response.StatusCode} {response.ReasonPhrase}");
                return response;
            }
            catch (Exception ex)
            {
                _output.WriteLine($"[{DateTimeOffset.UtcNow:O}] [{requestId}] EXCEPTION" + Environment.NewLine + ex);
                throw;
            }
        }
    }
}
