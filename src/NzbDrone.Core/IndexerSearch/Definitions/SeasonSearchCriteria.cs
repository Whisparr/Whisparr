namespace NzbDrone.Core.IndexerSearch.Definitions
{
    public class SeasonSearchCriteria : SceneSearchCriteriaBase
    {
        public int Year { get; set; }

        public override string ToString()
        {
            return string.Format("[{0} : S{1:00}]", Series.Title, Year);
        }
    }
}
