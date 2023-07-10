using System;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.IndexerSearch.Definitions
{
    public class SingleEpisodeSearchCriteria : SceneSearchCriteriaBase
    {
        public string EpisodeTitle { get; set; }
        public DateOnly? ReleaseDate { get; set; }
        public string Performer { get; set; }

        public override string ToString()
        {
            return string.Format("[{0} : {1} - {2}]", Series.Title, Performer, EpisodeTitle);
        }
    }
}
