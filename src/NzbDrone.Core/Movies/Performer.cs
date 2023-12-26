using System.Collections.Generic;

namespace NzbDrone.Core.Movies
{
    public class Performer
    {
        public string ForeignId { get; set; }
        public string Name { get; set; }
        public List<MediaCover.MediaCover> Images { get; set; }
    }
}
