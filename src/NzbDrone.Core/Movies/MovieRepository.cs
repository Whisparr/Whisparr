using System.Collections.Generic;
using System.Linq;
using Dapper;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Movies
{
    public interface IMovieRepository : IBasicRepository<Movie>
    {
        Movie FindByTmdbId(int tmdbId);
        Dictionary<int, string> AllMoviePaths();
    }

    public class MovieRepository : BasicRepository<Movie>, IMovieRepository
    {
        public MovieRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        public Movie FindByTmdbId(int tmdbId)
        {
            return Query(m => m.TmdbId == tmdbId).SingleOrDefault();
        }

        public Dictionary<int, string> AllMoviePaths()
        {
            using (var conn = _database.OpenConnection())
            {
                var strSql = "SELECT \"Id\" AS Key, \"Path\" AS Value FROM \"Movies\"";
                return conn.Query<KeyValuePair<int, string>>(strSql).ToDictionary(x => x.Key, x => x.Value);
            }
        }
    }
}
