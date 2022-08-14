using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.EnsureThat;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Movies.Events;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.RomanNumerals;

namespace NzbDrone.Core.Movies
{
    public interface IMovieService
    {
        Media GetMovie(int movieId);
        List<Media> GetMovies(IEnumerable<int> movieIds);
        PagingSpec<Media> Paged(PagingSpec<Media> pagingSpec);
        Media AddMovie(Media newMovie);
        List<Media> AddMovies(List<Media> newMovies);
        Media FindByTmdbId(int tmdbid);
        List<Media> FindByTmdbId(List<int> tmdbids);
        Media FindByTitle(string title);
        Media FindByTitle(string title, int year);
        Media FindByTitle(List<string> titles, int? year, List<string> otherTitles, List<Media> candidates);
        List<Media> FindByTitleCandidates(List<string> titles, out List<string> otherTitles);
        Media FindByPath(string path);
        Dictionary<int, string> AllMoviePaths();
        List<int> AllMovieTmdbIds();
        bool MovieExists(Media movie);
        List<Media> GetMoviesByFileId(int fileId);
        List<Media> GetMoviesBetweenDates(DateTime start, DateTime end, bool includeUnmonitored);
        PagingSpec<Media> MoviesWithoutFiles(PagingSpec<Media> pagingSpec);
        void SetFileId(Media movie, MediaFile movieFile);
        void DeleteMovie(int movieId, bool deleteFiles, bool addExclusion = false);
        void DeleteMovies(List<int> movieIds, bool deleteFiles, bool addExclusion = false);
        List<Media> GetAllMovies();
        Dictionary<int, List<int>> AllMovieTags();
        Media UpdateMovie(Media movie);
        List<Media> UpdateMovie(List<Media> movie, bool useExistingRelativeFolder);
        List<int> GetRecommendedTmdbIds();
        bool MoviePathExists(string folder);
        void RemoveAddOptions(Media movie);
    }

    public class MovieService : IMovieService, IHandle<MovieFileAddedEvent>,
                                               IHandle<MovieFileDeletedEvent>
    {
        private readonly IMediaRepository _movieRepository;
        private readonly IConfigService _configService;
        private readonly IEventAggregator _eventAggregator;
        private readonly IBuildMoviePaths _moviePathBuilder;
        private readonly Logger _logger;

        public MovieService(IMediaRepository movieRepository,
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

        public Media GetMovie(int movieId)
        {
            return _movieRepository.Get(movieId);
        }

        public List<Media> GetMovies(IEnumerable<int> movieIds)
        {
            return _movieRepository.Get(movieIds).ToList();
        }

        public PagingSpec<Media> Paged(PagingSpec<Media> pagingSpec)
        {
            return _movieRepository.GetPaged(pagingSpec);
        }

        public Media AddMovie(Media newMovie)
        {
            _movieRepository.Insert(newMovie);
            _eventAggregator.PublishEvent(new MovieAddedEvent(GetMovie(newMovie.Id)));

            return newMovie;
        }

        public List<Media> AddMovies(List<Media> newMovies)
        {
            _movieRepository.InsertMany(newMovies);

            _eventAggregator.PublishEvent(new MoviesImportedEvent(newMovies));

            return newMovies;
        }

        public Media FindByTitle(string title)
        {
            var candidates = FindByTitleCandidates(new List<string> { title }, out var otherTitles);

            return FindByTitle(new List<string> { title }, null, otherTitles, candidates);
        }

        public Media FindByTitle(string title, int year)
        {
            var candidates = FindByTitleCandidates(new List<string> { title }, out var otherTitles);

            return FindByTitle(new List<string> { title }, year, otherTitles, candidates);
        }

        public Media FindByTitle(List<string> cleanTitles, int? year, List<string> otherTitles, List<Media> candidates)
        {
            var result = candidates.Where(x => cleanTitles.Contains(x.MediaMetadata.Value.CleanTitle)).FirstWithYear(year);

            if (result == null)
            {
                result =
                    candidates.Where(movie => otherTitles.Contains(movie.MediaMetadata.Value.CleanTitle)).FirstWithYear(year);
            }

            if (result == null)
            {
                result = candidates
                    .Where(m => m.MediaMetadata.Value.AlternativeTitles.Any(t => cleanTitles.Contains(t.CleanTitle) ||
                                                        otherTitles.Contains(t.CleanTitle)))
                    .FirstWithYear(year);
            }

            if (result == null)
            {
                result = candidates
                    .Where(m => m.MediaMetadata.Value.Translations.Any(t => cleanTitles.Contains(t.CleanTitle) ||
                                                        otherTitles.Contains(t.CleanTitle)))
                    .FirstWithYear(year);
            }

            return result;
        }

        public List<Media> FindByTitleCandidates(List<string> titles, out List<string> otherTitles)
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

        public Media FindByTmdbId(int tmdbid)
        {
            return _movieRepository.FindByTmdbId(tmdbid);
        }

        public List<Media> FindByTmdbId(List<int> tmdbids)
        {
            return _movieRepository.FindByTmdbId(tmdbids);
        }

        public Media FindByPath(string path)
        {
            return _movieRepository.FindByPath(path);
        }

        public Dictionary<int, string> AllMoviePaths()
        {
            return _movieRepository.AllMoviePaths();
        }

        public List<int> AllMovieTmdbIds()
        {
            return _movieRepository.AllMovieTmdbIds();
        }

        public void DeleteMovie(int movieId, bool deleteFiles, bool addExclusion = false)
        {
            var movie = _movieRepository.Get(movieId);

            _movieRepository.Delete(movieId);
            _eventAggregator.PublishEvent(new MoviesDeletedEvent(new List<Media> { movie }, deleteFiles, addExclusion));
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

        public List<Media> GetAllMovies()
        {
            return _movieRepository.All().ToList();
        }

        public Dictionary<int, List<int>> AllMovieTags()
        {
            return _movieRepository.AllMovieTags();
        }

        public Media UpdateMovie(Media movie)
        {
            var storedMovie = GetMovie(movie.Id);

            var updatedMovie = _movieRepository.Update(movie);
            _eventAggregator.PublishEvent(new MovieEditedEvent(updatedMovie, storedMovie));

            return updatedMovie;
        }

        public List<Media> UpdateMovie(List<Media> movie, bool useExistingRelativeFolder)
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

        public bool MoviePathExists(string folder)
        {
            return _movieRepository.MoviePathExists(folder);
        }

        public void RemoveAddOptions(Media movie)
        {
            _movieRepository.SetFields(movie, s => s.AddOptions);
        }

        public void SetFileId(Media movie, MediaFile movieFile)
        {
            _movieRepository.SetFileId(movieFile.Id, movie.Id);
            _logger.Info("Assigning file [{0}] to movie [{1}]", movieFile.RelativePath, movie);
        }

        public List<Media> GetMoviesByFileId(int fileId)
        {
            return _movieRepository.GetMoviesByFileId(fileId);
        }

        public List<Media> GetMoviesBetweenDates(DateTime start, DateTime end, bool includeUnmonitored)
        {
            var movies = _movieRepository.MoviesBetweenDates(start.ToUniversalTime(), end.ToUniversalTime(), includeUnmonitored);

            return movies;
        }

        public PagingSpec<Media> MoviesWithoutFiles(PagingSpec<Media> pagingSpec)
        {
            var movieResult = _movieRepository.MoviesWithoutFiles(pagingSpec);

            return movieResult;
        }

        public bool MovieExists(Media movie)
        {
            Media result = null;

            if (movie.ForiegnId != 0)
            {
                result = _movieRepository.FindByTmdbId(movie.ForiegnId);
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

        public List<int> GetRecommendedTmdbIds()
        {
            return _movieRepository.GetRecommendations();
        }

        public void Handle(MovieFileAddedEvent message)
        {
            var movie = message.MovieFile.Movie;
            movie.MovieFileId = message.MovieFile.Id;
            _movieRepository.Update(movie);

            //_movieRepository.SetFileId(message.MovieFile.Id, message.MovieFile.Movie.Value.Id);
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
