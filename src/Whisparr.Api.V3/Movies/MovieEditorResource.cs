using System.Collections.Generic;

namespace Whisparr.Api.V3.Movies
{
    public class MovieEditorResource
    {
        public List<int> MovieIds { get; set; }
        public bool? Monitored { get; set; }
        public int? QualityProfileId { get; set; }
        public string RootFolderPath { get; set; }
        public List<int> Tags { get; set; }
        public ApplyTags ApplyTags { get; set; }
        public bool MoveFiles { get; set; }
        public bool DeleteFiles { get; set; }
        public bool AddImportExclusion { get; set; }
    }
}
