namespace Proxy.Configurations
{
    public class Firewall
    {
        public Firewall(bool enabled, Rule[] rules)
        {
            Enabled = enabled;
            Rules = rules;
        }

        public bool Enabled { get; private set; }
        public Rule[] Rules { get; private set; }
    }
}