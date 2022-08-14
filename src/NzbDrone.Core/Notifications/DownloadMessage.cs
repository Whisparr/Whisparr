using System.Collections.Generic;
using NzbDrone.Core.Download;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Movies;

namespace NzbDrone.Core.Notifications
{
    public class DownloadMessage
    {
        public string Message { get; set; }
        public Media Movie { get; set; }
        public MediaFile MovieFile { get; set; }
        public List<MediaFile> OldMovieFiles { get; set; }
        public string SourcePath { get; set; }
        public DownloadClientItemClientInfo DownloadClientInfo { get; set; }
        public string DownloadId { get; set; }

        public override string ToString()
        {
            return Message;
        }
    }
}
