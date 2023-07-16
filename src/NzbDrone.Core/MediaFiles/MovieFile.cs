using NzbDrone.Core.Datastore;
using NzbDrone.Core.Movies;

namespace NzbDrone.Core.MediaFiles
{
    public class MovieFile : MediaFile
    {
        public int MovieId { get; set; }
        public LazyLoaded<Movie> Movie { get; set; }

        public override string ToString()
        {
            return string.Format("[{0}] {1}", Id, RelativePath);
        }
    }
}
