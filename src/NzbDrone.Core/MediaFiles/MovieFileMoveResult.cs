using System.Collections.Generic;

namespace NzbDrone.Core.MediaFiles
{
    public class MovieFileMoveResult
    {
        public MovieFileMoveResult()
        {
            OldFiles = new List<MediaFile>();
        }

        public MediaFile MovieFile { get; set; }
        public List<MediaFile> OldFiles { get; set; }
    }
}
