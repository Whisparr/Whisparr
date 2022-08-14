using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Movies.AlternativeTitles;
using NzbDrone.Core.Movies.Translations;
using NzbDrone.Core.Profiles;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.Movies
{
    public interface IMediaRepository : IBasicRepository<Media>
    {
        bool MoviePathExists(string path);
        List<Media> FindByTitles(List<string> titles);
        Media FindByTmdbId(int tmdbid);
        List<Media> FindByTmdbId(List<int> tmdbids);
        List<Media> MoviesBetweenDates(DateTime start, DateTime end, bool includeUnmonitored);
        PagingSpec<Media> MoviesWithoutFiles(PagingSpec<Media> pagingSpec);
        List<Media> GetMoviesByFileId(int fileId);
        void SetFileId(int fileId, int movieId);
        PagingSpec<Media> MoviesWhereCutoffUnmet(PagingSpec<Media> pagingSpec, List<QualitiesBelowCutoff> qualitiesBelowCutoff);
        Media FindByPath(string path);
        Dictionary<int, string> AllMoviePaths();
        List<int> AllMovieTmdbIds();
        Dictionary<int, List<int>> AllMovieTags();
        List<int> GetRecommendations();
    }

    public class MediaRepository : BasicRepository<Media>, IMediaRepository
    {
        private readonly IProfileRepository _profileRepository;
        private readonly IAlternativeTitleRepository _alternativeTitleRepository;

        public MediaRepository(IMainDatabase database,
                               IProfileRepository profileRepository,
                               IAlternativeTitleRepository alternativeTitleRepository,
                               IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
            _profileRepository = profileRepository;
            _alternativeTitleRepository = alternativeTitleRepository;
        }

        protected override SqlBuilder Builder() => new SqlBuilder(_database.DatabaseType)
            .Join<Media, Profile>((m, p) => m.ProfileId == p.Id)
            .Join<Media, MediaMetadata>((m, p) => m.MovieMetadataId == p.Id)
            .LeftJoin<Media, MediaFile>((m, f) => m.Id == f.MovieId)
            .LeftJoin<MediaMetadata, AlternativeTitle>((mm, t) => mm.Id == t.MovieMetadataId);

        private Media Map(Dictionary<int, Media> dict, Media movie, Profile profile, MediaFile movieFile, AlternativeTitle altTitle = null, MovieTranslation translation = null)
        {
            Media movieEntry;

            if (!dict.TryGetValue(movie.Id, out movieEntry))
            {
                movieEntry = movie;
                movieEntry.Profile = profile;
                movieEntry.MovieFile = movieFile;
                dict.Add(movieEntry.Id, movieEntry);
            }

            if (altTitle != null)
            {
                movieEntry.MediaMetadata.Value.AlternativeTitles.Add(altTitle);
            }

            if (translation != null)
            {
                movieEntry.MediaMetadata.Value.Translations.Add(translation);
            }

            return movieEntry;
        }

        protected override List<Media> Query(SqlBuilder builder)
        {
            var movieDictionary = new Dictionary<int, Media>();

            _ = _database.QueryJoined<Media, Profile, MediaFile, AlternativeTitle>(
                builder,
                (movie, profile, file, altTitle) => Map(movieDictionary, movie, profile, file, altTitle));

            return movieDictionary.Values.ToList();
        }

        public override IEnumerable<Media> All()
        {
            // the skips the join on profile and alternative title and populates manually
            // to avoid repeatedly deserializing the same profile / movie
            var builder = new SqlBuilder(_database.DatabaseType)
                .LeftJoin<Media, MediaFile>((m, f) => m.MovieFileId == f.Id)
                .LeftJoin<Media, MediaMetadata>((m, f) => m.MovieMetadataId == f.Id);

            var profiles = _profileRepository.All().ToDictionary(x => x.Id);
            var titles = _alternativeTitleRepository.All()
                .GroupBy(x => x.MovieMetadataId)
                .ToDictionary(x => x.Key, y => y.ToList());

            return _database.QueryJoined<Media, MediaFile, MediaMetadata>(
                builder,
                (movie, file, metadata) =>
                {
                    movie.MovieFile = file;
                    movie.MediaMetadata = metadata;
                    movie.Profile = profiles[movie.ProfileId];

                    if (titles.TryGetValue(movie.MovieMetadataId, out var altTitles))
                    {
                        movie.MediaMetadata.Value.AlternativeTitles = altTitles;
                    }

                    return movie;
                });
        }

        public bool MoviePathExists(string path)
        {
            return Query(x => x.Path == path).Any();
        }

        public List<Media> FindByTitles(List<string> titles)
        {
            var distinct = titles.Distinct().ToList();

            var results = new List<Media>();

            results.AddRange(FindByMovieTitles(distinct));
            results.AddRange(FindByAltTitles(distinct));
            results.AddRange(FindByTransTitles(distinct));

            return results.DistinctBy(x => x.Id).ToList();
        }

        // This is a bit of a hack, but if you try to combine / rationalise these then
        // SQLite makes a mess of the query plan and ends up doing a table scan
        private List<Media> FindByMovieTitles(List<string> titles)
        {
            var movieDictionary = new Dictionary<int, Media>();

            var builder = new SqlBuilder(_database.DatabaseType)
                .Join<Media, Profile>((m, p) => m.ProfileId == p.Id)
                .Join<Media, MediaMetadata>((m, p) => m.MovieMetadataId == p.Id)
                .LeftJoin<Media, MediaFile>((m, f) => m.Id == f.MovieId)
                .Where<MediaMetadata>(x => titles.Contains(x.CleanTitle) || titles.Contains(x.CleanOriginalTitle));

            _ = _database.QueryJoined<Media, Profile, MediaFile>(
                builder,
                (movie, profile, file) => Map(movieDictionary, movie, profile, file));

            return movieDictionary.Values.ToList();
        }

        private List<Media> FindByAltTitles(List<string> titles)
        {
            var movieDictionary = new Dictionary<int, Media>();

            var builder = new SqlBuilder(_database.DatabaseType)
            .Join<AlternativeTitle, MediaMetadata>((t, mm) => t.MovieMetadataId == mm.Id)
            .Join<MediaMetadata, Media>((mm, m) => mm.Id == m.MovieMetadataId)
            .Join<Media, Profile>((m, p) => m.ProfileId == p.Id)
            .LeftJoin<Media, MediaFile>((m, f) => m.Id == f.MovieId)
            .Where<AlternativeTitle>(x => titles.Contains(x.CleanTitle));

            _ = _database.QueryJoined<AlternativeTitle, Profile, Media, MediaFile>(
                builder,
                (altTitle, profile, movie, file) =>
                {
                    _ = Map(movieDictionary, movie, profile, file, altTitle);
                    return null;
                });

            return movieDictionary.Values.ToList();
        }

        private List<Media> FindByTransTitles(List<string> titles)
        {
            var movieDictionary = new Dictionary<int, Media>();

            var builder = new SqlBuilder(_database.DatabaseType)
                .Join<MovieTranslation, MediaMetadata>((t, mm) => t.MovieMetadataId == mm.Id)
                .Join<MediaMetadata, Media>((mm, m) => mm.Id == m.MovieMetadataId)
                .Join<Media, Profile>((m, p) => m.ProfileId == p.Id)
                .LeftJoin<Media, MediaFile>((m, f) => m.Id == f.MovieId)
                .Where<MovieTranslation>(x => titles.Contains(x.CleanTitle));

            _ = _database.QueryJoined<MovieTranslation, Profile, Media, MediaFile>(
                builder,
                (trans, profile, movie, file) =>
                {
                    _ = Map(movieDictionary, movie, profile, file, null, trans);
                    return null;
                });

            return movieDictionary.Values.ToList();
        }

        public Media FindByTmdbId(int tmdbid)
        {
            return Query(x => x.MediaMetadata.Value.ForiegnId == tmdbid).FirstOrDefault();
        }

        public List<Media> FindByTmdbId(List<int> tmdbids)
        {
            return Query(x => tmdbids.Contains(x.ForiegnId));
        }

        public List<Media> GetMoviesByFileId(int fileId)
        {
            return Query(x => x.MovieFileId == fileId);
        }

        public void SetFileId(int fileId, int movieId)
        {
            SetFields(new Media { Id = movieId, MovieFileId = fileId }, movie => movie.MovieFileId);
        }

        public List<Media> MoviesBetweenDates(DateTime start, DateTime end, bool includeUnmonitored)
        {
            var builder = Builder()
                .Where<Media>(m => m.MediaMetadata.Value.DigitalRelease >= start && m.MediaMetadata.Value.DigitalRelease <= end);

            if (!includeUnmonitored)
            {
                builder.Where<Media>(x => x.Monitored == true);
            }

            return Query(builder);
        }

        public SqlBuilder MoviesWithoutFilesBuilder() => Builder()
            .Where<Media>(x => x.MovieFileId == 0);

        public PagingSpec<Media> MoviesWithoutFiles(PagingSpec<Media> pagingSpec)
        {
            pagingSpec.Records = GetPagedRecords(MoviesWithoutFilesBuilder(), pagingSpec, PagedQuery);
            pagingSpec.TotalRecords = GetPagedRecordCount(MoviesWithoutFilesBuilder().SelectCount(), pagingSpec);

            return pagingSpec;
        }

        public SqlBuilder MoviesWhereCutoffUnmetBuilder(List<QualitiesBelowCutoff> qualitiesBelowCutoff) => Builder()
                .Where<Media>(x => x.MovieFileId != 0)
                .Where(BuildQualityCutoffWhereClause(qualitiesBelowCutoff));

        public PagingSpec<Media> MoviesWhereCutoffUnmet(PagingSpec<Media> pagingSpec, List<QualitiesBelowCutoff> qualitiesBelowCutoff)
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
                    clauses.Add(string.Format($"(\"{_table}\".\"ProfileId\" = {profile.ProfileId} AND \"MediaFiles\".\"Quality\" LIKE '%_quality_: {belowCutoff},%')"));
                }
            }

            return string.Format("({0})", string.Join(" OR ", clauses));
        }

        public Media FindByPath(string path)
        {
            return Query(x => x.Path == path).FirstOrDefault();
        }

        public Dictionary<int, string> AllMoviePaths()
        {
            using (var conn = _database.OpenConnection())
            {
                var strSql = "SELECT \"Id\" AS \"Key\", \"Path\" AS \"Value\" FROM \"Media\"";
                return conn.Query<KeyValuePair<int, string>>(strSql).ToDictionary(x => x.Key, x => x.Value);
            }
        }

        public List<int> AllMovieTmdbIds()
        {
            using (var conn = _database.OpenConnection())
            {
                return conn.Query<int>("SELECT \"TmdbId\" FROM \"MovieMetadata\" JOIN \"Media\" ON (\"Media\".\"MovieMetadataId\" = \"MovieMetadata\".\"Id\")").ToList();
            }
        }

        public Dictionary<int, List<int>> AllMovieTags()
        {
            using (var conn = _database.OpenConnection())
            {
                var strSql = "SELECT \"Id\" AS \"Key\", \"Tags\" AS \"Value\" FROM \"Media\"";
                return conn.Query<KeyValuePair<int, List<int>>>(strSql).ToDictionary(x => x.Key, x => x.Value);
            }
        }

        public List<int> GetRecommendations()
        {
            var recommendations = new List<int>();

            if (_database.Version < new Version("3.9.0"))
            {
                return recommendations;
            }

            using (var conn = _database.OpenConnection())
            {
                if (_database.DatabaseType == DatabaseType.PostgreSQL)
                {
                    recommendations = conn.Query<int>(@"SELECT DISTINCT ""Rec"" FROM (
                                                    SELECT DISTINCT ""Rec"" FROM
                                                    (
                                                    SELECT DISTINCT CAST(""value"" AS INT) AS ""Rec"" FROM ""MovieMetadata"", json_array_elements_text((""MovieMetadata"".""Recommendations"")::json)
                                                    WHERE CAST(""value"" AS INT) NOT IN (SELECT ""TmdbId"" FROM ""MovieMetadata"" union SELECT ""TmdbId"" from ""ImportExclusions"" as sub1) LIMIT 10
                                                    ) as sub2
                                                    UNION
                                                    SELECT ""Rec"" FROM
                                                    (
                                                    SELECT CAST(""value"" AS INT) AS ""Rec"" FROM ""MovieMetadata"", json_array_elements_text((""MovieMetadata"".""Recommendations"")::json)
                                                    WHERE CAST(""value"" AS INT) NOT IN (SELECT ""TmdbId"" FROM ""MovieMetadata"" union SELECT ""TmdbId"" from ""ImportExclusions"" as sub2)
                                                    GROUP BY ""Rec"" ORDER BY count(*) DESC LIMIT 120
                                                    ) as sub4
                                                    ) as sub5
                                                    LIMIT 100;").ToList();
                }
                else
                {
                    recommendations = conn.Query<int>(@"SELECT DISTINCT ""Rec"" FROM (
                                                    SELECT DISTINCT ""Rec"" FROM
                                                    (
                                                    SELECT DISTINCT CAST(""j"".""value"" AS INT) AS ""Rec"" FROM ""MovieMetadata"" CROSS JOIN json_each(""MovieMetadata"".""Recommendations"") AS ""j""
                                                    WHERE ""Rec"" NOT IN (SELECT ""TmdbId"" FROM ""MovieMetadata"" union SELECT ""TmdbId"" from ""ImportExclusions"") LIMIT 10
                                                    )
                                                    UNION
                                                    SELECT ""Rec"" FROM
                                                    (
                                                    SELECT CAST(""j"".""value"" AS INT) AS ""Rec"" FROM ""MovieMetadata"" CROSS JOIN json_each(""MovieMetadata"".""Recommendations"") AS ""j""
                                                    WHERE ""Rec"" NOT IN (SELECT ""TmdbId"" FROM ""MovieMetadata"" union SELECT ""TmdbId"" from ""ImportExclusions"")
                                                    GROUP BY ""Rec"" ORDER BY count(*) DESC LIMIT 120
                                                    )
                                                    )
                                                    LIMIT 100;").ToList();
                }
            }

            return recommendations;
        }
    }
}
