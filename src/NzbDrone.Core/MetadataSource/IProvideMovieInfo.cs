using System;
using System.Collections.Generic;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Movies.Credits;

namespace NzbDrone.Core.MetadataSource
{
    public interface IProvideMovieInfo
    {
        MediaMetadata GetMovieByImdbId(string imdbId);
        Tuple<MediaMetadata, List<Credit>> GetMovieInfo(int tmdbId);
        List<MediaMetadata> GetBulkMovieInfo(List<int> tmdbIds);

        HashSet<int> GetChangedMovies(DateTime startTime);
    }
}
