using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Movies;

namespace NzbDrone.Core.Organizer
{
    public class SampleResult
    {
        public string FileName { get; set; }
        public Media Movie { get; set; }
        public MediaFile MovieFile { get; set; }
    }
}
