namespace Proxy.Configurations
{
    public class Server
    {
        public Server(int port, bool rejectHttpProxy)
        {
            Port = port;
            RejectHttpProxy = rejectHttpProxy;
        }

        public int Port { get; private set; }
        public bool RejectHttpProxy { get; private set; }
    }
}