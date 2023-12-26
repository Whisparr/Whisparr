using System;
using System.Collections.Generic;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Movies.Performers;
using NzbDrone.Core.Movies.Studios;

namespace NzbDrone.Core.MetadataSource
{
    public interface IProvideMovieInfo
    {
        MovieMetadata GetMovieByImdbId(string imdbId);
        Tuple<MovieMetadata, Studio, List<Performer>> GetMovieInfo(int tmdbId);
        Tuple<MovieMetadata, Studio, List<Performer>> GetSceneInfo(string stashId);
        List<MovieMetadata> GetBulkMovieInfo(List<int> tmdbIds);

        HashSet<string> GetChangedMovies(DateTime startTime);
    }
}
