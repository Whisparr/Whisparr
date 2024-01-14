using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Movies.AlternativeTitles;
using NzbDrone.Core.Profiles.Qualities;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.Movies
{
    public interface IMovieRepository : IBasicRepository<Movie>
    {
        bool MoviePathExists(string path);
        List<Movie> FindByTitles(List<string> titles);
        Movie FindByImdbId(string imdbid);
        Movie FindByTmdbId(int tmdbid);
        Movie FindByForeignId(string foreignId);
        List<Movie> FindByTmdbId(List<int> tmdbids);
        List<Movie> FindByStudioAndDate(string studioForeignId, string date);
        List<Movie> GetByStudioForeignId(string studioForeignId);
        List<Movie> GetByPerformerForeignId(string performerForeignId);
        List<Movie> MoviesBetweenDates(DateTime start, DateTime end, bool includeUnmonitored);
        PagingSpec<Movie> MoviesWithoutFiles(PagingSpec<Movie> pagingSpec);
        List<Movie> GetMoviesByFileId(int fileId);
        PagingSpec<Movie> MoviesWhereCutoffUnmet(PagingSpec<Movie> pagingSpec, List<QualitiesBelowCutoff> qualitiesBelowCutoff);
        Movie FindByPath(string path);
        Dictionary<int, string> AllMoviePaths();
        List<int> AllMovieTmdbIds();
        List<string> AllMovieForeignIds();
        Dictionary<int, List<int>> AllMovieTags();
        bool ExistsByMetadataId(int metadataId);
        HashSet<int> AllMovieWithCollectionsTmdbIds();
    }

    public class MovieRepository : BasicRepository<Movie>, IMovieRepository
    {
        private readonly IQualityProfileRepository _profileRepository;
        private readonly IAlternativeTitleRepository _alternativeTitleRepository;

        public MovieRepository(IMainDatabase database,
                               IQualityProfileRepository profileRepository,
                               IAlternativeTitleRepository alternativeTitleRepository,
                               IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
            _profileRepository = profileRepository;
            _alternativeTitleRepository = alternativeTitleRepository;
        }

        protected override SqlBuilder Builder() => new SqlBuilder(_database.DatabaseType)
            .Join<Movie, QualityProfile>((m, p) => m.QualityProfileId == p.Id)
            .Join<Movie, MovieMetadata>((m, p) => m.MovieMetadataId == p.Id)
            .LeftJoin<Movie, MovieFile>((m, f) => m.Id == f.MovieId);

        private Movie Map(Dictionary<int, Movie> dict, Movie movie, QualityProfile profile, MovieFile movieFile)
        {
            if (!dict.TryGetValue(movie.Id, out var movieEntry))
            {
                movieEntry = movie;
                movieEntry.QualityProfile = profile;
                movieEntry.MovieFile = movieFile;
                dict.Add(movieEntry.Id, movieEntry);
            }

            return movieEntry;
        }

        protected override List<Movie> Query(SqlBuilder builder)
        {
            var movieDictionary = new Dictionary<int, Movie>();

            _ = _database.QueryJoined<Movie, QualityProfile, MovieFile>(
                builder,
                (movie, profile, file) => Map(movieDictionary, movie, profile, file));

            return movieDictionary.Values.ToList();
        }

        public override IEnumerable<Movie> All()
        {
            // the skips the join on profile and alternative title and populates manually
            // to avoid repeatedly deserializing the same profile / movie
            var builder = new SqlBuilder(_database.DatabaseType)
                .LeftJoin<Movie, MovieFile>((m, f) => m.MovieFileId == f.Id)
                .LeftJoin<Movie, MovieMetadata>((m, f) => m.MovieMetadataId == f.Id);

            var profiles = _profileRepository.All().ToDictionary(x => x.Id);

            return _database.QueryJoined<Movie, MovieFile, MovieMetadata>(
                builder,
                (movie, file, metadata) =>
                {
                    movie.MovieFile = file;
                    movie.MovieMetadata = metadata;
                    movie.QualityProfile = profiles[movie.QualityProfileId];

                    return movie;
                });
        }

        public bool MoviePathExists(string path)
        {
            return Query(x => x.Path == path).Any();
        }

        public List<Movie> FindByTitles(List<string> titles)
        {
            var distinct = titles.Distinct().ToList();

            var results = new List<Movie>();

            results.AddRange(FindByMovieTitles(distinct));

            return results.DistinctBy(x => x.Id).ToList();
        }

        // This is a bit of a hack, but if you try to combine / rationalise these then
        // SQLite makes a mess of the query plan and ends up doing a table scan
        private List<Movie> FindByMovieTitles(List<string> titles)
        {
            var movieDictionary = new Dictionary<int, Movie>();

            var builder = new SqlBuilder(_database.DatabaseType)
                .Join<Movie, QualityProfile>((m, p) => m.QualityProfileId == p.Id)
                .Join<Movie, MovieMetadata>((m, p) => m.MovieMetadataId == p.Id)
                .LeftJoin<Movie, MovieFile>((m, f) => m.Id == f.MovieId)
                .Where<MovieMetadata>(x => titles.Contains(x.CleanTitle));

            _ = _database.QueryJoined<Movie, QualityProfile, MovieFile>(
                builder,
                (movie, profile, file) => Map(movieDictionary, movie, profile, file));

            return movieDictionary.Values.ToList();
        }

        public List<Movie> FindByStudioAndDate(string studioForeignId, string date)
        {
            var builder = new SqlBuilder(_database.DatabaseType)
                .Join<Movie, QualityProfile>((m, p) => m.QualityProfileId == p.Id)
                .Join<Movie, MovieMetadata>((m, p) => m.MovieMetadataId == p.Id)
                .LeftJoin<Movie, MovieFile>((m, f) => m.Id == f.MovieId)
                .Where<MovieMetadata>(x => x.StudioForeignId == studioForeignId && x.ReleaseDate == date);

            return _database.QueryJoined<Movie, QualityProfile, MovieMetadata, MovieFile>(
                builder,
                (movie, profile, metadata, file) =>
                {
                    movie.QualityProfile = profile;
                    movie.MovieMetadata = metadata;
                    movie.MovieFile = file;

                    return movie;
                }).AsList();
        }

        public List<Movie> GetByStudioForeignId(string studioForeignId)
        {
            var builder = new SqlBuilder(_database.DatabaseType)
                .Join<Movie, MovieMetadata>((m, p) => m.MovieMetadataId == p.Id)
                .Where<MovieMetadata>(x => x.StudioForeignId == studioForeignId);

            return _database.QueryJoined<Movie, MovieMetadata>(
                builder,
                (movie, metadata) =>
                {
                    movie.MovieMetadata = metadata;

                    return movie;
                }).AsList();
        }

        public List<Movie> GetByPerformerForeignId(string performerForeignId)
        {
            var builder = new SqlBuilder(_database.DatabaseType)
                .Join<Movie, MovieMetadata>((m, p) => m.MovieMetadataId == p.Id)
                .Where($"\"MovieMetadata\".\"Credits\" LIKE \"%{performerForeignId}%\"");

            return _database.QueryJoined<Movie, MovieMetadata>(
                builder,
                (movie, metadata) =>
                {
                    movie.MovieMetadata = metadata;

                    return movie;
                }).AsList();
        }

        public Movie FindByImdbId(string imdbid)
        {
            var imdbIdWithPrefix = Parser.Parser.NormalizeImdbId(imdbid);
            return imdbIdWithPrefix == null ? null : Query(x => x.MovieMetadata.Value.ImdbId == imdbIdWithPrefix).FirstOrDefault();
        }

        public Movie FindByTmdbId(int tmdbid)
        {
            return Query(x => x.MovieMetadata.Value.TmdbId == tmdbid).FirstOrDefault();
        }

        public Movie FindByForeignId(string foreignId)
        {
            return Query(x => x.MovieMetadata.Value.ForeignId == foreignId).FirstOrDefault();
        }

        public List<Movie> FindByTmdbId(List<int> tmdbids)
        {
            return Query(x => tmdbids.Contains(x.TmdbId));
        }

        public List<Movie> GetMoviesByFileId(int fileId)
        {
            return Query(x => x.MovieFileId == fileId);
        }

        public List<Movie> MoviesBetweenDates(DateTime start, DateTime end, bool includeUnmonitored)
        {
            var builder = Builder()
                .Where<Movie>(m => m.MovieMetadata.Value.ReleaseDateUtc >= start && m.MovieMetadata.Value.ReleaseDateUtc <= end);

            if (!includeUnmonitored)
            {
                builder.Where<Movie>(x => x.Monitored == true);
            }

            return Query(builder);
        }

        public SqlBuilder MoviesWithoutFilesBuilder() => Builder()
            .Where<Movie>(x => x.MovieFileId == 0);

        public PagingSpec<Movie> MoviesWithoutFiles(PagingSpec<Movie> pagingSpec)
        {
            pagingSpec.Records = GetPagedRecords(MoviesWithoutFilesBuilder(), pagingSpec, PagedQuery);
            pagingSpec.TotalRecords = GetPagedRecordCount(MoviesWithoutFilesBuilder().SelectCount(), pagingSpec);

            return pagingSpec;
        }

        public SqlBuilder MoviesWhereCutoffUnmetBuilder(List<QualitiesBelowCutoff> qualitiesBelowCutoff) => Builder()
                .Where<Movie>(x => x.MovieFileId != 0)
                .Where(BuildQualityCutoffWhereClause(qualitiesBelowCutoff));

        public PagingSpec<Movie> MoviesWhereCutoffUnmet(PagingSpec<Movie> pagingSpec, List<QualitiesBelowCutoff> qualitiesBelowCutoff)
        {
            pagingSpec.Records = GetPagedRecords(MoviesWhereCutoffUnmetBuilder(qualitiesBelowCutoff), pagingSpec, PagedQuery);
            pagingSpec.TotalRecords = GetPagedRecordCount(MoviesWhereCutoffUnmetBuilder(qualitiesBelowCutoff).SelectCount(), pagingSpec);

            return pagingSpec;
        }

        private string BuildQualityCutoffWhereClause(List<QualitiesBelowCutoff> qualitiesBelowCutoff)
        {
            var clauses = new List<string>();

            foreach (var profile in qualitiesBelowCutoff)
            {
                foreach (var belowCutoff in profile.QualityIds)
                {
                    clauses.Add(string.Format($"(\"{_table}\".\"QualityProfileId\" = {profile.ProfileId} AND \"MovieFiles\".\"Quality\" LIKE '%_quality_: {belowCutoff},%')"));
                }
            }

            return string.Format("({0})", string.Join(" OR ", clauses));
        }

        public Movie FindByPath(string path)
        {
            return Query(x => x.Path == path).FirstOrDefault();
        }

        public Dictionary<int, string> AllMoviePaths()
        {
            using (var conn = _database.OpenConnection())
            {
                var strSql = "SELECT \"Id\" AS \"Key\", \"Path\" AS \"Value\" FROM \"Movies\"";
                return conn.Query<KeyValuePair<int, string>>(strSql).ToDictionary(x => x.Key, x => x.Value);
            }
        }

        public List<int> AllMovieTmdbIds()
        {
            using (var conn = _database.OpenConnection())
            {
                return conn.Query<int>("SELECT \"TmdbId\" FROM \"MovieMetadata\" JOIN \"Movies\" ON (\"Movies\".\"MovieMetadataId\" = \"MovieMetadata\".\"Id\")").ToList();
            }
        }

        public List<string> AllMovieForeignIds()
        {
            using (var conn = _database.OpenConnection())
            {
                return conn.Query<string>("SELECT \"ForeignId\" FROM \"MovieMetadata\" JOIN \"Movies\" ON (\"Movies\".\"MovieMetadataId\" = \"MovieMetadata\".\"Id\")").ToList();
            }
        }

        public Dictionary<int, List<int>> AllMovieTags()
        {
            using (var conn = _database.OpenConnection())
            {
                var strSql = "SELECT \"Id\" AS \"Key\", \"Tags\" AS \"Value\" FROM \"Movies\" WHERE \"Tags\" IS NOT NULL";
                return conn.Query<KeyValuePair<int, List<int>>>(strSql).ToDictionary(x => x.Key, x => x.Value);
            }
        }

        public bool ExistsByMetadataId(int metadataId)
        {
            var movies = Query(x => x.MovieMetadataId == metadataId);

            return movies.Any();
        }

        public HashSet<int> AllMovieWithCollectionsTmdbIds()
        {
            using (var conn = _database.OpenConnection())
            {
                return conn.Query<int>("SELECT \"TmdbId\" FROM \"MovieMetadata\" JOIN \"Movies\" ON (\"Movies\".\"MovieMetadataId\" = \"MovieMetadata\".\"Id\") WHERE \"CollectionTmdbId\" > 0").ToHashSet();
            }
        }
    }
}
