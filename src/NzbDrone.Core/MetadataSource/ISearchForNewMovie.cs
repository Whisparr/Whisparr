using System.Collections.Generic;
using NzbDrone.Core.Movies;

namespace NzbDrone.Core.MetadataSource
{
    public interface ISearchForNewMovie
    {
        List<Movie> SearchForNewMovie(string title);
        List<Movie> SearchForNewScene(string title);
        List<object> SearchForNewEntity(string title, ItemType itemType);

        MovieMetadata MapMovieToTmdbMovie(MovieMetadata movie);
    }
}
