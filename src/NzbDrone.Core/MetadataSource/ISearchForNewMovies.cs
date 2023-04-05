using System.Collections.Generic;
using NzbDrone.Core.Movies;

namespace NzbDrone.Core.MetadataSource
{
    public interface ISearchForNewMovies
    {
        List<Movie> SearchForNewMovies(string title);
    }
}
