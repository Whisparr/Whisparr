using System;
using System.Collections.Generic;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Movies.Collections;

namespace NzbDrone.Core.MetadataSource
{
    public interface IProvideMovieInfo
    {
        MovieMetadata GetMovieByImdbId(string imdbId);
        MovieMetadata GetMovieInfo(int tmdbId);
        MovieMetadata GetSceneInfo(string stashId);
        MovieCollection GetCollectionInfo(int tmdbId);
        List<MovieMetadata> GetBulkMovieInfo(List<int> tmdbIds);

        HashSet<string> GetChangedMovies(DateTime startTime);
    }
}
