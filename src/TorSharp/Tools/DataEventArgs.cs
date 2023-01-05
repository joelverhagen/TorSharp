using System;

namespace Knapcode.TorSharp.Tools
{
    public class DataEventArgs : EventArgs
    {
        public DataEventArgs(string executablePath, string data)
        {
            ExecutablePath = executablePath ?? throw new ArgumentNullException(nameof(executablePath));
            Data = data;
        }

        public string ExecutablePath { get; }
        public string Data { get; }
    }
}