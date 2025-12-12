using System;
using System.Text.RegularExpressions;

namespace NzbDrone.Core.Parser
{
    public class ExternalIdPattern
    {
        private readonly Regex _regex;
        private readonly Func<Match, string> _extractor;

        public ExternalIdPattern(string pattern, Func<Match, string> extractor)
        {
            _regex = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
            _extractor = extractor;
        }

        public string TryExtract(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return null;
            }

            var match = _regex.Match(input);

            if (match.Success)
            {
                return _extractor(match);
            }

            return null;
        }

        public override string ToString()
        {
            return _regex.ToString();
        }
    }
}
