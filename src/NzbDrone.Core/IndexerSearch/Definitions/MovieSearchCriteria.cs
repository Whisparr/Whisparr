using NzbDrone.Core.Movies;

namespace NzbDrone.Core.IndexerSearch.Definitions
{
    public class MovieSearchCriteria : SearchCriteriaBase
    {
        public Movie Movie { get; set; }
    }
}
