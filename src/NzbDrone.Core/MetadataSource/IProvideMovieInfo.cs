using NzbDrone.Core.Movies;

namespace NzbDrone.Core.MetadataSource
{
    public interface IProvideMovieInfo
    {
        Movie GetMovieInfo(int tmdbMovieId);
    }
}
