using System.Collections.Generic;

namespace NzbDrone.Core.MediaFiles
{
    public class RenameEpisodeFilePreview
    {
        public int SeriesId { get; set; }
        public int SeasonNumber { get; set; }
        public List<string> ReleaseDates { get; set; }
        public int EpisodeFileId { get; set; }
        public string ExistingPath { get; set; }
        public string NewPath { get; set; }
    }
}
