using System.Collections.Generic;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Movies;

namespace NzbDrone.Core.Notifications
{
    public class DeleteMessage
    {
        public string Message { get; set; }
        public Media Movie { get; set; }
        public MediaFile MovieFile { get; set; }

        public DeleteMediaFileReason Reason { get; set; }

        public override string ToString()
        {
            return Message;
        }
    }
}
