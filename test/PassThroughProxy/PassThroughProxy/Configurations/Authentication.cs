namespace Proxy.Configurations
{
    public class Authentication
    {
        public Authentication(bool enabled, string username, string password)
        {
            Username = username;
            Password = password;
            Enabled = enabled;
        }

        public bool Enabled { get; private set; }
        public string Username { get; private set; }
        public string Password { get; private set; }
    }
}