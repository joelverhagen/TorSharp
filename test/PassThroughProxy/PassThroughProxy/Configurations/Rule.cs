using System.Text.RegularExpressions;

namespace Proxy.Configurations
{
    public class Rule
    {
        public Rule(string pattern, ActionEnum action)
        {
            Action = action;
            Pattern = new Regex(pattern, RegexOptions.Compiled);
        }


        public Regex Pattern { get; private set; }

        public ActionEnum Action { get; private set; }
    }

    public enum ActionEnum
    {
        Deny,
        Allow
    }
}