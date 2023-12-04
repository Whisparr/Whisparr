using System;

namespace NzbDrone.Core.IndexerSearch.Definitions
{
    public class SceneSearchCriteria : SearchCriteriaBase
    {
        public string SiteTitle { get; set; }
        public DateOnly? ReleaseDate { get; set; }
        public string Performer { get; set; }

        public override string ToString()
        {
            return string.Format("[{0}]", Movie.Title);
        }
    }
}
