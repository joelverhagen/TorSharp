using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Proxy.Configurations;
using Proxy.Headers;
using Proxy.Sessions;

namespace Proxy.Handlers
{
    public class AuthenticationHandler : IHandler
    {
        private static readonly AuthenticationHandler Self = new AuthenticationHandler();

        private AuthenticationHandler()
        {
        }

        public async Task<ExitReason> Run(SessionContext context)
        {
            if (!IsAuthenticationRequired(context))
            {
                return ExitReason.AuthenticationNotRequired;
            }

            if (!IsProxyAuthorizationHeaderPresent(context.Header))
            {
                await SendProxyAuthenticationRequired(context.ClientStream);
                return ExitReason.TerminationRequired;
            }

            if (IsProxyAuthorizationCredentialsCorrect(context))
            {
                return ExitReason.Authenticated;
            }

            await SendProxyAuthenticationInvalid(context.ClientStream);
            return ExitReason.TerminationRequired;
        }

        public static AuthenticationHandler Instance()
        {
            return Self;
        }

        private static bool IsAuthenticationRequired(SessionContext context)
        {
            return context.Configuration.Authentication.Enabled;
        }

        private static bool IsProxyAuthorizationHeaderPresent(HttpHeader httpHeader)
        {
            return httpHeader.ArrayList.Any(@string => @string.StartsWith("Proxy-Authorization: Basic", StringComparison.OrdinalIgnoreCase));
        }

        private static bool IsProxyAuthorizationCredentialsCorrect(SessionContext context)
        {
            const string key = "Proxy-Authorization: Basic";

            var value = context.Header.ArrayList
                .First(@string => @string.StartsWith(key, StringComparison.OrdinalIgnoreCase))
                .Substring(key.Length)
                .Trim();

            return Encoding.ASCII.GetString(Convert.FromBase64String(value)) == $"{context.Configuration.Authentication.Username}:{context.Configuration.Authentication.Password}";
        }

        private static async Task SendProxyAuthenticationRequired(Stream stream)
        {
            var bytes = Encoding.ASCII.GetBytes("HTTP/1.1 407 Proxy Authentication Required\r\nProxy-Authenticate: Basic realm=\"Pass Through Proxy\"\r\nConnection: close\r\n\r\n");
            await stream.WriteAsync(bytes, 0, bytes.Length);
        }

        private static async Task SendProxyAuthenticationInvalid(Stream stream)
        {
            var bytes = Encoding.ASCII.GetBytes("HTTP/1.1 407 Proxy Authentication Invalid\r\nProxy-Authenticate: Basic realm=\"Pass Through Proxy\"\r\nConnection: close\r\n\r\n");
            await stream.WriteAsync(bytes, 0, bytes.Length);
        }
    }
}