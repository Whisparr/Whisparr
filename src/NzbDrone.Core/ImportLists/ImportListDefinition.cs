using System;
using Equ;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.ImportLists
{
    public class ImportListDefinition : ProviderDefinition, IEquatable<ImportListDefinition>
    {
        private static readonly MemberwiseEqualityComparer<ImportListDefinition> Comparer = MemberwiseEqualityComparer<ImportListDefinition>.ByProperties;

        public bool EnableAutomaticAdd { get; set; }
        public bool SearchForMissingEpisodes { get; set; }
        public ImportListMonitorTypes ShouldMonitor { get; set; }
        public MonitorTypes SiteMonitorType { get; set; }
        public NewItemMonitorTypes MonitorNewItems { get; set; }
        public int QualityProfileId { get; set; }
        public string RootFolderPath { get; set; }

        [MemberwiseEqualityIgnore]
        public override bool Enable => EnableAutomaticAdd;

        [MemberwiseEqualityIgnore]
        public ImportListStatus Status { get; set; }

        [MemberwiseEqualityIgnore]
        public ImportListType ListType { get; set; }

        [MemberwiseEqualityIgnore]
        public TimeSpan MinRefreshInterval { get; set; }

        public bool Equals(ImportListDefinition other)
        {
            return Comparer.Equals(this, other);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as ImportListDefinition);
        }

        public override int GetHashCode()
        {
            return Comparer.GetHashCode(this);
        }
    }

    public enum ImportListMonitorTypes
    {
        None,
        SpecificEpisode,
        EntireSite
    }
}
