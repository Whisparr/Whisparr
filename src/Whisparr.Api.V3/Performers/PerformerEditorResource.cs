using System.Collections.Generic;

namespace Whisparr.Api.V3.Performers
{
    public class PerformerEditorResource
    {
        public List<int> PerformerIds { get; set; }
        public bool? Monitored { get; set; }
        public int? QualityProfileId { get; set; }
        public string RootFolderPath { get; set; }
        public List<int> Tags { get; set; }
        public ApplyTags ApplyTags { get; set; }
    }
}
