using System.Collections.Generic;
using NzbDrone.Core.Movies;

namespace NzbDrone.Core.MetadataSource
{
    public interface ISearchForNewMovie
    {
        List<Media> SearchForNewMovie(string title);

        MediaMetadata MapMovieToTmdbMovie(MediaMetadata movie);
    }
}
