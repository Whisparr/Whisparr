using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Movies.Events;
using NzbDrone.Core.Movies.Performers;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.RomanNumerals;

namespace NzbDrone.Core.Movies
{
    public interface IMovieService
    {
        Movie GetMovie(int movieId);
        List<Movie> GetMovies(IEnumerable<int> movieIds);
        PagingSpec<Movie> Paged(PagingSpec<Movie> pagingSpec);
        Movie AddMovie(Movie newMovie);
        List<Movie> AddMovies(List<Movie> newMovies);
        List<Movie> FindByIds(List<int> ids);
        Movie FindByImdbId(string imdbid);
        Movie FindByTmdbId(int tmdbid);
        Movie FindByForeignId(string foreignId);
        Movie FindByTitle(string title);
        Movie FindByTitle(string title, int year);
        Movie FindByTitle(List<string> titles, int? year, List<string> otherTitles, List<Movie> candidates);
        List<Movie> FindByTitleCandidates(List<string> titles, out List<string> otherTitles);
        Movie FindByStudioAndReleaseDate(string studioForeignId, string releaseDate, string releaseTokens);
        List<Movie> GetByStudioForeignId(string studioForeignId);
        List<Movie> GetByPerformerForeignId(string performerForeignId);
        Movie FindByPath(string path);
        Dictionary<int, string> AllMoviePaths();
        List<int> AllMovieIds();
        List<int> AllMovieTmdbIds();
        List<string> AllMovieForeignIds();
        bool MovieExists(Movie movie);
        List<Movie> GetMoviesByFileId(int fileId);
        List<Movie> GetMoviesBetweenDates(DateTime start, DateTime end, bool includeUnmonitored);
        PagingSpec<Movie> MoviesWithoutFiles(PagingSpec<Movie> pagingSpec);
        void DeleteMovie(int movieId, bool deleteFiles, bool addExclusion = false);
        void DeleteMovies(List<int> movieIds, bool deleteFiles, bool addExclusion = false);
        List<Movie> GetAllMovies();
        Dictionary<int, List<int>> AllMovieTags();
        Movie UpdateMovie(Movie movie);
        List<Movie> UpdateMovie(List<Movie> movie, bool useExistingRelativeFolder);
        void UpdateLastSearchTime(Movie movie);
        bool MoviePathExists(string folder);
        void RemoveAddOptions(Movie movie);
        bool ExistsByMetadataId(int metadataId);
    }

    public class MovieService : IMovieService, IHandle<MovieFileAddedEvent>,
                                               IHandle<MovieFileDeletedEvent>
    {
        private readonly IMovieRepository _movieRepository;
        private readonly IConfigService _configService;
        private readonly IEventAggregator _eventAggregator;
        private readonly IBuildMoviePaths _moviePathBuilder;
        private readonly Logger _logger;

        public MovieService(IMovieRepository movieRepository,
                            IEventAggregator eventAggregator,
                            IConfigService configService,
                            IBuildMoviePaths moviePathBuilder,
                            Logger logger)
        {
            _movieRepository = movieRepository;
            _eventAggregator = eventAggregator;
            _configService = configService;
            _moviePathBuilder = moviePathBuilder;
            _logger = logger;
        }

        public Movie GetMovie(int movieId)
        {
            return _movieRepository.Get(movieId);
        }

        public List<Movie> GetMovies(IEnumerable<int> movieIds)
        {
            return _movieRepository.Get(movieIds).ToList();
        }

        public PagingSpec<Movie> Paged(PagingSpec<Movie> pagingSpec)
        {
            return _movieRepository.GetPaged(pagingSpec);
        }

        public Movie AddMovie(Movie newMovie)
        {
            var movie = _movieRepository.Insert(newMovie);

            _eventAggregator.PublishEvent(new MovieAddedEvent(GetMovie(movie.Id)));

            return movie;
        }

        public List<Movie> AddMovies(List<Movie> newMovies)
        {
            _movieRepository.InsertMany(newMovies);

            _eventAggregator.PublishEvent(new MoviesImportedEvent(newMovies));

            return newMovies;
        }

        public Movie FindByTitle(string title)
        {
            var candidates = FindByTitleCandidates(new List<string> { title }, out var otherTitles);

            return FindByTitle(new List<string> { title }, null, otherTitles, candidates);
        }

        public Movie FindByTitle(string title, int year)
        {
            var candidates = FindByTitleCandidates(new List<string> { title }, out var otherTitles);

            return FindByTitle(new List<string> { title }, year, otherTitles, candidates);
        }

        public Movie FindByTitle(List<string> titles, int? year, List<string> otherTitles, List<Movie> candidates)
        {
            var cleanTitles = titles.Select(t => t.CleanMovieTitle().ToLowerInvariant());

            var result = candidates.Where(x => cleanTitles.Contains(x.MovieMetadata.Value.CleanTitle))
                .AllWithYear(year)
                .ToList();

            if (result == null || result.Count == 0)
            {
                result =
                    candidates.Where(movie => otherTitles.Contains(movie.MovieMetadata.Value.CleanTitle)).AllWithYear(year).ToList();
            }

            return ReturnSingleMovieOrThrow(result.ToList());
        }

        public List<Movie> FindByTitleCandidates(List<string> titles, out List<string> otherTitles)
        {
            var lookupTitles = new List<string>();
            otherTitles = new List<string>();

            foreach (var title in titles)
            {
                var cleanTitle = title.CleanMovieTitle().ToLowerInvariant();
                var romanTitle = cleanTitle;
                var arabicTitle = cleanTitle;

                foreach (var arabicRomanNumeral in RomanNumeralParser.GetArabicRomanNumeralsMapping())
                {
                    var arabicNumber = arabicRomanNumeral.ArabicNumeralAsString;
                    var romanNumber = arabicRomanNumeral.RomanNumeral;

                    romanTitle = romanTitle.Replace(arabicNumber, romanNumber);
                    arabicTitle = arabicTitle.Replace(romanNumber, arabicNumber);
                }

                romanTitle = romanTitle.ToLowerInvariant();

                otherTitles.AddRange(new List<string> { arabicTitle, romanTitle });
                lookupTitles.AddRange(new List<string> { cleanTitle, arabicTitle, romanTitle });
            }

            return _movieRepository.FindByTitles(lookupTitles);
        }

        public List<Movie> FindByIds(List<int> ids)
        {
            return _movieRepository.FindByIds(ids).ToList();
        }

        public Movie FindByImdbId(string imdbid)
        {
            return _movieRepository.FindByImdbId(imdbid);
        }

        public Movie FindByTmdbId(int tmdbid)
        {
            return _movieRepository.FindByTmdbId(tmdbid);
        }

        public Movie FindByForeignId(string foreignId)
        {
            return _movieRepository.FindByForeignId(foreignId);
        }

        public Movie FindByPath(string path)
        {
            return _movieRepository.FindByPath(path);
        }

        public Dictionary<int, string> AllMoviePaths()
        {
            return _movieRepository.AllMoviePaths();
        }

        public List<int> AllMovieIds()
        {
            return _movieRepository.AllMovieIds();
        }

        public List<int> AllMovieTmdbIds()
        {
            return _movieRepository.AllMovieTmdbIds();
        }

        public List<string> AllMovieForeignIds()
        {
            return _movieRepository.AllMovieForeignIds();
        }

        public List<Movie> GetByStudioForeignId(string studioForeignId)
        {
            return _movieRepository.GetByStudioForeignId(studioForeignId);
        }

        public List<Movie> GetByPerformerForeignId(string performerForeignId)
        {
            return _movieRepository.GetByPerformerForeignId(performerForeignId);
        }

        public void DeleteMovie(int movieId, bool deleteFiles, bool addExclusion = false)
        {
            var movie = _movieRepository.Get(movieId);

            _movieRepository.Delete(movieId);
            _eventAggregator.PublishEvent(new MoviesDeletedEvent(new List<Movie> { movie }, deleteFiles, addExclusion));

            _logger.Info("Deleted movie {0}", movie);
        }

        public void DeleteMovies(List<int> movieIds, bool deleteFiles, bool addExclusion = false)
        {
            var moviesToDelete = _movieRepository.Get(movieIds).ToList();

            _movieRepository.DeleteMany(movieIds);

            _eventAggregator.PublishEvent(new MoviesDeletedEvent(moviesToDelete, deleteFiles, addExclusion));

            foreach (var movie in moviesToDelete)
            {
                _logger.Info("Deleted movie {0}", movie);
            }
        }

        public List<Movie> GetAllMovies()
        {
            return _movieRepository.All().ToList();
        }

        public Dictionary<int, List<int>> AllMovieTags()
        {
            return _movieRepository.AllMovieTags();
        }

        public Movie UpdateMovie(Movie movie)
        {
            var storedMovie = GetMovie(movie.Id);

            var updatedMovie = _movieRepository.Update(movie);

            _eventAggregator.PublishEvent(new MovieEditedEvent(updatedMovie, storedMovie));

            return updatedMovie;
        }

        public List<Movie> UpdateMovie(List<Movie> movie, bool useExistingRelativeFolder)
        {
            _logger.Debug("Updating {0} movie", movie.Count);
            foreach (var m in movie)
            {
                _logger.Trace("Updating: {0}", m.Title);

                if (!m.RootFolderPath.IsNullOrWhiteSpace())
                {
                    m.Path = _moviePathBuilder.BuildPath(m, useExistingRelativeFolder);

                    _logger.Trace("Changing path for {0} to {1}", m.Title, m.Path);
                }
                else
                {
                    _logger.Trace("Not changing path for: {0}", m.Title);
                }
            }

            _movieRepository.UpdateMany(movie);
            _logger.Debug("{0} movie updated", movie.Count);

            return movie;
        }

        public void UpdateLastSearchTime(Movie movie)
        {
            _movieRepository.SetFields(movie, e => e.LastSearchTime);
        }

        public bool MoviePathExists(string folder)
        {
            return _movieRepository.MoviePathExists(folder);
        }

        public void RemoveAddOptions(Movie movie)
        {
            _movieRepository.SetFields(movie, s => s.AddOptions);
        }

        public List<Movie> GetMoviesByFileId(int fileId)
        {
            return _movieRepository.GetMoviesByFileId(fileId);
        }

        public List<Movie> GetMoviesBetweenDates(DateTime start, DateTime end, bool includeUnmonitored)
        {
            var movies = _movieRepository.MoviesBetweenDates(start.ToUniversalTime(), end.ToUniversalTime(), includeUnmonitored);

            return movies;
        }

        public PagingSpec<Movie> MoviesWithoutFiles(PagingSpec<Movie> pagingSpec)
        {
            var movieResult = _movieRepository.MoviesWithoutFiles(pagingSpec);

            return movieResult;
        }

        public bool MovieExists(Movie movie)
        {
            Movie result = null;

            if (movie.TmdbId != 0)
            {
                result = _movieRepository.FindByTmdbId(movie.TmdbId);
                if (result != null)
                {
                    return true;
                }
            }

            if (movie.ImdbId.IsNotNullOrWhiteSpace())
            {
                result = _movieRepository.FindByImdbId(movie.ImdbId);
                if (result != null)
                {
                    return true;
                }
            }

            if (movie.Title.IsNotNullOrWhiteSpace())
            {
                if (movie.Year > 1850)
                {
                    result = FindByTitle(movie.Title.CleanMovieTitle(), movie.Year);
                    if (result != null)
                    {
                        return true;
                    }
                }
                else
                {
                    result = FindByTitle(movie.Title.CleanMovieTitle());
                    if (result != null)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public bool ExistsByMetadataId(int metadataId)
        {
            return _movieRepository.ExistsByMetadataId(metadataId);
        }

        public Movie FindByStudioAndReleaseDate(string studioForeignId, string releaseDate, string releaseTokens)
        {
            if (string.IsNullOrEmpty(studioForeignId))
            {
                studioForeignId = string.Empty;
            }

            if (string.IsNullOrEmpty(releaseDate))
            {
                releaseDate = string.Empty;
            }

            if (string.IsNullOrEmpty(releaseTokens))
            {
                releaseTokens = string.Empty;
            }

            var movies = new List<Movie>();

            if (releaseDate.IsNotNullOrWhiteSpace())
            {
                movies = _movieRepository.FindByStudioAndDate(studioForeignId, releaseDate);
            }

            if (movies == null || !movies.Any())
            {
                return null;
            }

            var parsedMovieTitle = Parser.Parser.NormalizeEpisodeTitle(releaseTokens);

            if (parsedMovieTitle.IsNotNullOrWhiteSpace())
            {
                var matches = MatchMovies(parsedMovieTitle, releaseDate, movies);

                if (matches.Count == 1)
                {
                    return matches.First().Key;
                }

                movies = matches.Keys.ToList();
            }

            _logger.Debug("Multiple scenes with the same release date found. Date: {0}", releaseDate);
            return null;
        }

        private Dictionary<Movie, MovieParseMatchType> MatchMovies(string parsedMovieTitle, string releaseDate, List<Movie> movies)
        {
            var matches = new Dictionary<Movie, MovieParseMatchType>();

            _logger.Debug("Checking {0} against {1} movies", parsedMovieTitle, movies.Count);

            foreach (var movie in movies)
            {
                var cleanTitle = movie.Title.IsNotNullOrWhiteSpace() ? Parser.Parser.NormalizeEpisodeTitle(movie.Title) : string.Empty;

                // If parsed title matches title, consider a match
                if (cleanTitle.IsNotNullOrWhiteSpace() && parsedMovieTitle.Equals(cleanTitle))
                {
                    _logger.Debug("Match {0} against {1} [Title]", parsedMovieTitle, cleanTitle);
                    matches.Add(movie, MovieParseMatchType.Title);
                    continue;
                }

                if (cleanTitle.IsNotNullOrWhiteSpace() && Parser.Parser.StripSpaces(parsedMovieTitle).Equals(Parser.Parser.StripSpaces(cleanTitle)))
                {
                    _logger.Debug("Match {0} against {1} [Title]", parsedMovieTitle, cleanTitle);
                    matches.Add(movie, MovieParseMatchType.Title);
                    continue;
                }

                var cleanPerformers = movie.MovieMetadata.Value.Credits.Select(a => Parser.Parser.NormalizeEpisodeTitle(a.Performer.Name))
                                                                       .Where(x => x.IsNotNullOrWhiteSpace());

                if (cleanPerformers.Empty())
                {
                    continue;
                }

                // If parsed title matches performer, consider a match
                if (cleanPerformers.Any(p => p.IsNotNullOrWhiteSpace() && parsedMovieTitle.Equals(p)))
                {
                    _logger.Debug("Match {0} against {1} [Performers]", parsedMovieTitle, cleanPerformers.Join(", "));
                    matches.Add(movie, MovieParseMatchType.PerformersTitle);
                    continue;
                }

                var cleanCharacters = movie.MovieMetadata.Value.Credits.Select(a => Parser.Parser.NormalizeEpisodeTitle(a.Character))
                                                                        .Where(x => x.IsNotNullOrWhiteSpace());

                // If parsed title matches character, consider a match
                if (cleanCharacters.Any() && cleanCharacters.Any(c => c.IsNotNullOrWhiteSpace() && parsedMovieTitle.Equals(c)))
                {
                    _logger.Debug("Match {0} against {1} [Characters]", parsedMovieTitle, cleanCharacters.Join(", "));
                    matches.Add(movie, MovieParseMatchType.CharactersTitle);
                    continue;
                }

                var cleanFemalePerformers = movie.MovieMetadata.Value.Credits.Where(a => a.Performer.Gender == Gender.Female)
                                                                             .Select(a => Parser.Parser.NormalizeEpisodeTitle(a.Performer.Name))
                                                                             .Where(x => x.IsNotNullOrWhiteSpace()).ToList();

                // If all female performers are in title, consider a match
                if (cleanFemalePerformers.Any() && cleanFemalePerformers.All(x => parsedMovieTitle.Contains(x)))
                {
                    _logger.Debug("Match {0} against {1} [Female Performers]", parsedMovieTitle, cleanFemalePerformers.Join(", "));
                    matches.Add(movie, MovieParseMatchType.Performers);
                    continue;
                }

                var cleanFemaleCharacters = movie.MovieMetadata.Value.Credits.Where(a => a.Performer.Gender == Gender.Female)
                                                                             .Select(a => Parser.Parser.NormalizeEpisodeTitle(a.Character))
                                                                             .Where(x => x.IsNotNullOrWhiteSpace()).ToList();

                // If all female performers are in title, consider a match
                if (cleanFemaleCharacters.Any() && cleanFemalePerformers.All(x => parsedMovieTitle.Contains(x)))
                {
                    _logger.Debug("Match {0} against {1} [Female Characters]", parsedMovieTitle, cleanFemaleCharacters.Join(", "));
                    matches.Add(movie, MovieParseMatchType.Characters);
                    continue;
                }

                if (cleanTitle.IsNullOrWhiteSpace())
                {
                    continue;
                }

                // If parsed title contains a performer and the title then consider a match
                if (cleanPerformers.Any(x => parsedMovieTitle.Contains(x)) && parsedMovieTitle.Contains(cleanTitle))
                {
                    _logger.Debug("Match {0} against {1} {2} [Title & Performer]", parsedMovieTitle, cleanTitle, cleanPerformers.Join(", "));
                    matches.Add(movie, MovieParseMatchType.PerformerTitle);
                    continue;
                }

                // If parsed title contains a performer and the title then consider a match
                if (cleanCharacters.Any() && cleanCharacters.Any(x => parsedMovieTitle.Contains(x)) && parsedMovieTitle.Contains(cleanTitle))
                {
                    _logger.Debug("Match {0} against {1} {2} [Title & Character]", parsedMovieTitle, cleanTitle, cleanCharacters.Join(", "));
                    matches.Add(movie, MovieParseMatchType.CharacterTitle);
                    continue;
                }

                // If parsed title contains all performer and the not title then consider a match
                if (cleanPerformers.All(x => parsedMovieTitle.Contains(x)) && !parsedMovieTitle.Contains(cleanTitle))
                {
                    _logger.Debug("Match {0} against {1} {2} [Performers & NOT Title]", parsedMovieTitle, cleanTitle, cleanPerformers.Join(", "));
                    matches.Add(movie, MovieParseMatchType.PerformersNotTitle);
                    continue;
                }

                // If parsed title contains all character and the not title then consider a match
                if (cleanCharacters.Any() && cleanCharacters.All(x => parsedMovieTitle.Contains(x)) && !parsedMovieTitle.Contains(cleanTitle))
                {
                    _logger.Debug("Match {0} against {1} {2} [Characters & NOT Title]", parsedMovieTitle, cleanTitle, cleanCharacters.Join(", "));
                    matches.Add(movie, MovieParseMatchType.CharactersNotTitle);
                    continue;
                }
            }

            // Find the best match
            if (matches.Count > 1)
            {
                foreach (var movieMatchType in (MovieParseMatchType[])Enum.GetValues(typeof(MovieParseMatchType)))
                {
                    var filteredMatches = matches.Where(m => m.Value > movieMatchType).ToDictionary(x => x.Key, x => x.Value);
                    if (filteredMatches.Count == 1)
                    {
                        matches = filteredMatches;
                        break;
                    }
                }
            }

            return matches;
        }

        private Movie ReturnSingleMovieOrThrow(List<Movie> movies)
        {
            if (movies.Count == 0)
            {
                return null;
            }

            if (movies.Count == 1)
            {
                return movies.First();
            }

            throw new MultipleMoviesFoundException(movies, "Expected one movie, but found {0}. Matching movies: {1}", movies.Count, string.Join(",", movies));
        }

        public void Handle(MovieFileAddedEvent message)
        {
            var movie = message.MovieFile.Movie;
            movie.MovieFileId = message.MovieFile.Id;
            _movieRepository.Update(movie);

            // _movieRepository.SetFileId(message.MovieFile.Id, message.MovieFile.Movie.Value.Id);
            _logger.Info("Assigning file [{0}] to movie [{1}]", message.MovieFile.RelativePath, message.MovieFile.Movie);
        }

        public void Handle(MovieFileDeletedEvent message)
        {
            foreach (var movie in GetMoviesByFileId(message.MovieFile.Id))
            {
                _logger.Debug("Detaching movie {0} from file.", movie.Id);
                movie.MovieFileId = 0;

                if (message.Reason != DeleteMediaFileReason.Upgrade && _configService.AutoUnmonitorPreviouslyDownloadedMovies)
                {
                    movie.Monitored = false;
                }

                UpdateMovie(movie);
            }
        }
    }
}
