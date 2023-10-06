using System;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.ImportLists
{
    public class ImportListDefinition : ProviderDefinition
    {
        public bool EnableAutomaticAdd { get; set; }
        public bool SearchForMissingEpisodes { get; set; }
        public ImportListMonitorTypes ShouldMonitor { get; set; }
        public MonitorTypes SiteMonitorType { get; set; }
        public int QualityProfileId { get; set; }
        public string RootFolderPath { get; set; }

        public override bool Enable => EnableAutomaticAdd;

        public ImportListStatus Status { get; set; }
        public ImportListType ListType { get; set; }
        public TimeSpan MinRefreshInterval { get; set; }
    }

    public enum ImportListMonitorTypes
    {
        None,
        SpecificEpisode,
        EntireSite
    }
}
