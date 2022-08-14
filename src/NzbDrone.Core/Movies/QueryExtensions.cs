using System.Collections.Generic;
using System.Linq;

namespace NzbDrone.Core.Movies
{
    public static class EnumerableExtensions
    {
        public static Media FirstWithYear(this IEnumerable<Media> query, int? year)
        {
            return year.HasValue ? query.FirstOrDefault(movie => movie.Year == year || movie.MediaMetadata.Value.SecondaryYear == year) : query.FirstOrDefault();
        }
    }
}
