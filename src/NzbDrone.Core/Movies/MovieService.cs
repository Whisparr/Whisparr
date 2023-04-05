using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Movies.Events;

namespace NzbDrone.Core.Movies
{
    public interface IMovieService
    {
        Movie GetMovie(int movieId);
        List<Movie> GetMovies(IEnumerable<int> movieIds);
        Movie AddMovie(Movie newMovie);
        List<Movie> AddMovies(List<Movie> newMovies);
        Movie FindByTmdbId(int tmdbId);
        List<Movie> GetAllMovies();
        Dictionary<int, string> GetAllMoviePaths();
        Movie UpdateMovie(Movie movie, bool publishUpdatedEvent = true);
        List<Movie> UpdateMovies(List<Movie> movies, bool useExistingRelativeFolder);
    }

    public class MovieService : IMovieService
    {
        private readonly IMovieRepository _movieRepository;
        private readonly IEventAggregator _eventAggregator;
        private readonly IBuildMoviePaths _moviePathBuilder;
        private readonly Logger _logger;

        public MovieService(IMovieRepository movieRepository,
                            IEventAggregator eventAggregator,
                            IBuildMoviePaths moviePathBuilder,
                            Logger logger)
        {
            _movieRepository = movieRepository;
            _eventAggregator = eventAggregator;
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

        public Movie AddMovie(Movie newMovie)
        {
            _movieRepository.Insert(newMovie);
            _eventAggregator.PublishEvent(new MovieAddedEvent(GetMovie(newMovie.Id)));

            return newMovie;
        }

        public List<Movie> AddMovies(List<Movie> newMovies)
        {
            _movieRepository.InsertMany(newMovies);
            _eventAggregator.PublishEvent(new MoviesImportedEvent(newMovies.Select(s => s.Id).ToList()));

            return newMovies;
        }

        public Movie FindByTmdbId(int tmdbId)
        {
            return _movieRepository.FindByTmdbId(tmdbId);
        }

        public List<Movie> GetAllMovies()
        {
            return _movieRepository.All().ToList();
        }

        public Dictionary<int, string> GetAllMoviePaths()
        {
            return _movieRepository.AllMoviePaths();
        }

        // updateEpisodesToMatchSeason is an override for EpisodeMonitoredService to use so a change via Season pass doesn't get nuked by the seasons loop.
        // TODO: Remove when seasons are split from series (or we come up with a better way to address this)
        public Movie UpdateMovie(Movie movie, bool publishUpdatedEvent = true)
        {
            var storedMovie = GetMovie(movie.Id);

            // Never update AddOptions when updating a series, keep it the same as the existing stored series.
            movie.AddOptions = storedMovie.AddOptions;

            var updatedMovie = _movieRepository.Update(movie);
            if (publishUpdatedEvent)
            {
                _eventAggregator.PublishEvent(new MovieEditedEvent(updatedMovie, storedMovie));
            }

            return updatedMovie;
        }

        public List<Movie> UpdateMovies(List<Movie> movies, bool useExistingRelativeFolder)
        {
            _logger.Debug("Updating {0} movies", movies.Count);

            foreach (var s in movies)
            {
                _logger.Trace("Updating: {0}", s.Title);

                if (!s.RootFolderPath.IsNullOrWhiteSpace())
                {
                    s.Path = _moviePathBuilder.BuildPath(s, useExistingRelativeFolder);

                    _logger.Trace("Changing path for {0} to {1}", s.Title, s.Path);
                }
                else
                {
                    _logger.Trace("Not changing path for: {0}", s.Title);
                }
            }

            _movieRepository.UpdateMany(movies);
            _logger.Debug("{0} movies updated", movies.Count);

            return movies;
        }
    }
}
