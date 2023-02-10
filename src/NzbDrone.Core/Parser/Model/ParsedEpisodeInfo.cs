using System.Collections.Generic;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.Parser.Model
{
    public class ParsedEpisodeInfo
    {
        public string ReleaseTitle { get; set; }
        public string SeriesTitle { get; set; } // Site Title
        public SeriesTitleInfo SeriesTitleInfo { get; set; }
        public QualityModel Quality { get; set; }
        public string AirDate { get; set; } // Release Date
        public List<Language> Languages { get; set; }
        public string ReleaseGroup { get; set; }
        public string ReleaseHash { get; set; }
        public int SeasonPart { get; set; }
        public string ReleaseTokens { get; set; }

        public ParsedEpisodeInfo()
        {
            Languages = new List<Language>();
        }

        public bool IsDaily
        {
            get
            {
                return !string.IsNullOrWhiteSpace(AirDate);
            }

            private set
            {
            }
        }

        public override string ToString()
        {
            string episodeString = "[Unknown Episode]";

            if (IsDaily)
            {
                episodeString = string.Format("{0}", AirDate);
            }

            return string.Format("{0} - {1} {2}", SeriesTitle, episodeString, Quality);
        }
    }
}
