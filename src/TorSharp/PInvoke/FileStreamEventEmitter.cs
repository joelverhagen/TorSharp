using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32.SafeHandles;

namespace Knapcode.TorSharp.PInvoke
{
    internal class FileStreamEventEmitter : IDisposable
    {
        private FileStream _fileStream;
        private StreamReader _streamReader;
        private readonly CancellationTokenSource _cts;
        private readonly Action<string> _onData;

        public FileStreamEventEmitter(IntPtr handle, Action<string> onData)
        {
            _cts = new CancellationTokenSource();
            _onData = onData;
            var _ = Task.Run(async () =>
            {
                _fileStream = new FileStream(new SafeFileHandle(handle, ownsHandle: true), FileAccess.Read);
                _streamReader = new StreamReader(_fileStream);

                string line;
                do
                {
                    try
                    {
                        line = await _streamReader.ReadLineAsync().ConfigureAwait(false);
                        _onData(line);
                    }
                    catch (Exception)
                    {
                        break;
                    }
                }
                while (!_cts.IsCancellationRequested && line != null);
            });
        }

        public void Dispose()
        {
            _cts.Cancel();
            _fileStream.Dispose();
        }
    }
}
