using System.Collections.Generic;
using System.Linq;

namespace NzbDrone.Core.MovieStats
{
    public interface IMovieStatisticsService
    {
        List<MovieStatistics> MovieStatistics();
        List<MovieStatistics> MovieStatistics(List<int> ids);
        MovieStatistics MovieStatistics(int movieId);
    }

    public class MovieStatisticsService : IMovieStatisticsService
    {
        private readonly IMovieStatisticsRepository _movieStatisticsRepository;

        public MovieStatisticsService(IMovieStatisticsRepository movieStatisticsRepository)
        {
            _movieStatisticsRepository = movieStatisticsRepository;
        }

        public List<MovieStatistics> MovieStatistics()
        {
            var movieStatistics = _movieStatisticsRepository.MovieStatistics();

            return movieStatistics.GroupBy(m => m.MovieId).Select(m => m.First()).ToList();
        }

        public List<MovieStatistics> MovieStatistics(List<int> ids)
        {
            var movieStatistics = _movieStatisticsRepository.MovieStatistics(ids);
            return movieStatistics.GroupBy(m => m.MovieId).Select(m => m.First()).ToList();
        }

        public MovieStatistics MovieStatistics(int movieId)
        {
            var stats = _movieStatisticsRepository.MovieStatistics(movieId);

            if (stats == null || stats.Count == 0)
            {
                return new MovieStatistics();
            }

            return stats.First();
        }
    }
}
