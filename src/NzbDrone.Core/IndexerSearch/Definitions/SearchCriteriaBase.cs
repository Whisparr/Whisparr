using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NzbDrone.Common.EnsureThat;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Movies;

namespace NzbDrone.Core.IndexerSearch.Definitions
{
    public abstract class SearchCriteriaBase
    {
        private static readonly Regex SpecialCharacter = new Regex(@"[`']", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        // Remove all dots not part of a date.
        // The regex pattern @"(?<!\d{2})\.(?!\d{2})" is used to match dots that are not part of a date in the yy.MM.dd format.
        // (?<!\d{2}) is a negative lookbehind that ensures the dot is not preceded by two digits.
        // (?!\d{2}) is a negative lookahead that ensures the dot is not followed by two digits.
        private static readonly Regex Dots = new Regex(@"(?<!\d{2})\.(?!\d{2})", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex NonWord = new Regex(@"[\W]", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex BeginningThe = new Regex(@"^the\s", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public Movie Movie { get; set; }
        public List<string> SceneTitles { get; set; }
        public virtual bool MonitoredEpisodesOnly { get; set; }
        public virtual bool UserInvokedSearch { get; set; }
        public virtual bool InteractiveSearch { get; set; }

        public List<string> CleanSceneTitles => SceneTitles.Select(GetCleanSceneTitle).Distinct().ToList();

        public static string GetCleanSceneTitle(string title)
        {
            Ensure.That(title, () => title).IsNotNullOrWhiteSpace();

            var cleanTitle = BeginningThe.Replace(title, string.Empty);

            cleanTitle = cleanTitle.Replace("&", "and");
            cleanTitle = SpecialCharacter.Replace(cleanTitle, "");
            cleanTitle = Dots.Replace(cleanTitle, "");
            cleanTitle = NonWord.Replace(cleanTitle, "+");

            // remove any repeating +s
            cleanTitle = Regex.Replace(cleanTitle, @"\+{2,}", "+");
            cleanTitle = cleanTitle.RemoveAccent();
            return cleanTitle.Trim('+', ' ');
        }
    }
}
