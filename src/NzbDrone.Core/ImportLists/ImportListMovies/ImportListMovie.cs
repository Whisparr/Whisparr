using NzbDrone.Core.Datastore;
using NzbDrone.Core.Movies;

namespace NzbDrone.Core.ImportLists.ImportListMovies
{
    public class ImportListMovie : ModelBase
    {
        public ImportListMovie()
        {
            MovieMetadata = new MediaMetadata();
        }

        public int ListId { get; set; }
        public int MovieMetadataId { get; set; }
        public LazyLoaded<MediaMetadata> MovieMetadata { get; set; }

        public string Title
        {
            get { return MovieMetadata.Value.Title; }
            set { MovieMetadata.Value.Title = value; }
        }

        public int ForiegnId
        {
            get { return MovieMetadata.Value.ForiegnId; }
            set { MovieMetadata.Value.ForiegnId = value; }
        }

        public int Year
        {
            get { return MovieMetadata.Value.Year; }
            set { MovieMetadata.Value.Year = value; }
        }
    }
}
