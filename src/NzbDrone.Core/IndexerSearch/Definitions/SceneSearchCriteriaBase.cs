using System.Collections.Generic;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.IndexerSearch.Definitions
{
    public abstract class SceneSearchCriteriaBase : SearchCriteriaBase
    {
        public Series Series { get; set; }
        public List<Episode> Episodes { get; set; }
        public virtual bool MonitoredEpisodesOnly { get; set; }
    }
}
