using System.Collections.Generic;

namespace Whisparr.Api.V3.Series
{
    public class SeriesEditorResource
    {
        public List<int> SeriesIds { get; set; }
        public bool? Monitored { get; set; }
        public int? QualityProfileId { get; set; }
        public string RootFolderPath { get; set; }
        public List<int> Tags { get; set; }
        public ApplyTags ApplyTags { get; set; }
        public bool MoveFiles { get; set; }
        public bool DeleteFiles { get; set; }
        public bool AddImportListExclusion { get; set; }
    }

    public enum ApplyTags
    {
        Add,
        Remove,
        Replace
    }
}
