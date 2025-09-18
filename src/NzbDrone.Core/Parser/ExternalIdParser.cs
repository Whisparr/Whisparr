using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NLog;

namespace NzbDrone.Core.Parser
{
    public static class ExternalIdParser
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        // List of regex patterns to match External IDs - easily extensible
        private static readonly List<Regex> ExternalIdPatterns = new List<Regex>
        {
            // Pattern for IDs in brackets like [SKMJ-649], (ABC-123), {XYZ-456}, [SKMJ_649], [SKMJ.649]
            new Regex(@"[\[\(\{]([A-Z]+[_\.\-\s]?\d+)[\]\)\}]", RegexOptions.IgnoreCase | RegexOptions.Compiled),

            // Pattern for IDs at the start of title like "SKMJ-649 Title here"
            new Regex(@"^([A-Z]+[-\s]?\d+)\s+", RegexOptions.IgnoreCase | RegexOptions.Compiled),

            // Add more patterns here as needed
        };

        public static string ExtractExternalId(string releaseTitle)
        {
            if (string.IsNullOrWhiteSpace(releaseTitle))
            {
                return null;
            }

            // Try each pattern until we find a match
            foreach (var pattern in ExternalIdPatterns)
            {
                var match = pattern.Match(releaseTitle);
                if (match.Success && match.Groups.Count > 1)
                {
                    var externalId = match.Groups[1].Value.Trim();
                    Logger.Debug("Extracted External ID '{0}' from release title '{1}' using pattern '{2}'", externalId, releaseTitle, pattern.ToString());
                    return externalId;
                }
            }

            Logger.Debug("No External ID found in release title: '{0}'", releaseTitle);
            return null;
        }

        public static void AddPattern(string pattern, RegexOptions options = RegexOptions.IgnoreCase | RegexOptions.Compiled)
        {
            try
            {
                var regex = new Regex(pattern, options);
                ExternalIdPatterns.Add(regex);
                Logger.Info("Added new External ID pattern: {0}", pattern);
            }
            catch (System.Exception ex)
            {
                Logger.Error(ex, "Failed to add External ID pattern: {0}", pattern);
            }
        }

        public static List<string> GetPatterns()
        {
            return ExternalIdPatterns.Select(p => p.ToString()).ToList();
        }

        public static bool ContainsExternalId(string releaseTitle, string externalId)
        {
            if (string.IsNullOrWhiteSpace(releaseTitle) || string.IsNullOrWhiteSpace(externalId))
            {
                return false;
            }

            var extractedId = ExtractExternalId(releaseTitle);
            return string.Equals(extractedId, externalId, System.StringComparison.OrdinalIgnoreCase);
        }
    }
}
