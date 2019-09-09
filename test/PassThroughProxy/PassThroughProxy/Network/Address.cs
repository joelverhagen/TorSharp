namespace Proxy.Network
{
    public class Address
    {
        public Address(string hostname, int port)
        {
            Hostname = hostname;
            Port = port;
        }

        public string Hostname { get; }
        public int Port { get; }

        protected bool Equals(Address other)
        {
            return string.Equals(Hostname, other.Hostname) && Port == other.Port;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Address) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Hostname?.GetHashCode() ?? 0)*397) ^ Port;
            }
        }
    }
}