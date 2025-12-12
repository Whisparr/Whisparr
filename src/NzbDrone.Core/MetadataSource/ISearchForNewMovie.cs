using System.Collections.Generic;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Movies.Performers;
using NzbDrone.Core.Movies.Studios;

namespace NzbDrone.Core.MetadataSource
{
    public interface ISearchForNewMovie
    {
        List<Movie> SearchForNewMovie(string title);
        List<Movie> SearchForNewScene(string title);
        List<Performer> SearchForNewPerformer(string title);
        List<Studio> SearchForNewStudio(string title);
        List<object> SearchForNewEntity(string title, ItemType itemType);
        MovieMetadata MapMovieToTmdbMovie(MovieMetadata movie);
    }
}
