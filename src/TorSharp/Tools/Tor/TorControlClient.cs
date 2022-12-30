using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Knapcode.TorSharp.Tools.Tor
{
    public class TorControlClient : IDisposable
    {
        private const string SuccessResponse = "250 OK";
        private const string ClosingConnectionResponse = "250 closing connection";
        private const int BufferSize = 4096;

        private TcpClient _tcpClient;
        private StreamReader _reader;
        private StreamWriter _writer;

        public async Task ConnectAsync(string hostname, int port)
        {
            _tcpClient = new TcpClient();
            await _tcpClient.ConnectAsync(hostname, port).ConfigureAwait(false);
            var networkStream = _tcpClient.GetStream();
            _reader = new StreamReader(networkStream, Encoding.ASCII, false, BufferSize, true);
            _writer = new StreamWriter(networkStream, Encoding.ASCII, BufferSize, true);
        }

        public async Task AuthenticateAsync(string password)
        {
            var command = password != null ? $"AUTHENTICATE \"{password}\"" : "AUTHENTICATE";
            await SendCommandAsync(command, SuccessResponse).ConfigureAwait(false);
        }

        public async Task SignalAsync(string signal)
        {
            await SendCommandAsync($"SIGNAL {signal}", SuccessResponse).ConfigureAwait(false);
        }

        public async Task<List<KeyValuePair<string, string>>> GetInfoAsync(IEnumerable<string> keywords)
        {
            if (_tcpClient == null)
            {
                throw new TorControlException("The Tor control client has not connected.");
            }

            var commandBuilder = new StringBuilder("GETINFO");
            var keywordCount = 0;
            foreach (var keyword in keywords)
            {
                commandBuilder.AppendFormat(" {0}", keyword);
                keywordCount++;
            };

            if (keywordCount == 0)
            {
                throw new ArgumentException("At least one keyword must be provided to GETINFO.");
            }

            await _writer.WriteLineAsync(commandBuilder.ToString()).ConfigureAwait(false);
            await _writer.FlushAsync().ConfigureAwait(false);

            var pairs = new List<KeyValuePair<string, string>>();
            while (true)
            {
                var value = await ReadGetInfoValueOrNullAsync().ConfigureAwait(false);
                if (value == null)
                {
                    break;
                }

                pairs.Add(new KeyValuePair<string, string>(value.Keyword, value.Value));
            }

            return pairs;
        }

        public async Task QuitAsync()
        {
            await SendCommandAsync("QUIT", ClosingConnectionResponse).ConfigureAwait(false);
        }

        public async Task CleanCircuitsAsync()
        {
            await SignalAsync("NEWNYM").ConfigureAwait(false);
        }

        public async Task<long> GetTrafficReadAsync()
        {
            return await GetInfoAsLongAsync("traffic/read").ConfigureAwait(false);
        }

        public async Task<long> GetTrafficWrittenAsync()
        {
            return await GetInfoAsLongAsync("traffic/written").ConfigureAwait(false);
        }

        public void Dispose()
        {
            _tcpClient?.Close();
            _reader?.Dispose();
            _writer?.Dispose();
        }

        public void Close()
        {
            Dispose();
        }

        private async Task<long> GetInfoAsLongAsync(string keyword)
        {
            var values = await GetInfoAsync(new[] { keyword }).ConfigureAwait(false);
            var value = values.Single(p => p.Key == keyword).Value;
            return long.Parse(value);
        }

        public async Task<string> SendCommandAsync(string command, string expectedResponse)
        {
            if (_tcpClient == null)
            {
                throw new TorControlException("The Tor control client has not connected.");
            }

            await _writer.WriteLineAsync(command).ConfigureAwait(false);
            await _writer.FlushAsync().ConfigureAwait(false);

            var response = await _reader.ReadLineAsync().ConfigureAwait(false);
            if (response != expectedResponse)
            {
                throw new TorControlException($"The command to authenticate failed with error: {response}");
            }

            return response;
        }

        private async Task<GetInfoValue> ReadGetInfoValueOrNullAsync()
        {
            var firstLine = await _reader.ReadLineAsync().ConfigureAwait(false);
            if (firstLine == SuccessResponse)
            {
                return null;
            }

            if (!firstLine.StartsWith("250"))
            {
                throw new TorControlException($"The GETINFO command failed with error: {firstLine}");
            }

            const int startOfKeyword = 4;
            var endOfKeyword = firstLine.IndexOf('=');
            if (endOfKeyword <= startOfKeyword)
            {
                throw new TorControlException($"Invalid 250 line recieved from the GETINFO: {firstLine}");
            }

            var keyword = firstLine.Substring(startOfKeyword, endOfKeyword - startOfKeyword);

            switch (firstLine[3])
            {
                case '-':
                    var value = firstLine.Substring(endOfKeyword + 1);
                    return new GetInfoValue(keyword, value);
                case '+':
                    var valueBuilder = new StringBuilder();
                    while (true)
                    {
                        var currentLine = await _reader.ReadLineAsync().ConfigureAwait(false);
                        if (currentLine == ".")
                        {
                            break;
                        }

                        if (valueBuilder.Length > 0)
                        {
                            valueBuilder.Append("\r\n");
                        }

                        valueBuilder.Append(currentLine);
                    }
                    return new GetInfoValue(keyword, valueBuilder.ToString());
                default:
                    throw new TorControlException($"Unexpected 250 line recieved from the GETINFO: {firstLine}");
            }
        }

        private class GetInfoValue
        {
            public GetInfoValue(string keyword, string value)
            {
                Keyword = keyword;
                Value = value;
            }

            public string Keyword { get; }
            public string Value { get; }
        }
    }
}