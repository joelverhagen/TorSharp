using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Proxy.Headers
{
    public class HttpHeaderStream
    {
        private static readonly string[] Delimiter = {"\r", "\n", "\r", "\n"};

        private static readonly HttpHeaderStream Self = new HttpHeaderStream();

        private HttpHeaderStream()
        {
        }

        public static HttpHeaderStream Instance()
        {
            return Self;
        }

        public async Task<HttpHeader> GetHeader(Stream client)
        {
            using (var memoryStream = await GetStream(client))
            {
                var array = memoryStream.ToArray();

                return array.Length == 0 ? null : new HttpHeader(array);
            }
        }

        private static async Task<MemoryStream> GetStream(Stream client)
        {
            var memoryStream = new MemoryStream();
            var readBuffer = new byte[1];

            int bytesRead;
            var counter = 0;

            do
            {
                bytesRead = await client.ReadAsync(readBuffer, 0, 1);
                await memoryStream.WriteAsync(readBuffer, 0, bytesRead);

                counter = Encoding.ASCII.GetString(readBuffer) == Delimiter[counter] ? counter + 1 : 0;

                if (counter == Delimiter.Length)
                {
                    return memoryStream;
                }
            } while (bytesRead > 0);

            return memoryStream;
        }
    }
}