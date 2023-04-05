using System.Collections.Generic;
using System.Linq;

namespace NzbDrone.Core.SeriesStats
{
    public interface ISeriesStatisticsService
    {
        List<SeriesStatistics> SeriesStatistics();
        List<MovieStatistics> MovieStatistics();
        SeriesStatistics SeriesStatistics(int seriesId);
        MovieStatistics MovieStatistics(int movieId);
    }

    public class SeriesStatisticsService : ISeriesStatisticsService
    {
        private readonly ISeriesStatisticsRepository _seriesStatisticsRepository;

        public SeriesStatisticsService(ISeriesStatisticsRepository seriesStatisticsRepository)
        {
            _seriesStatisticsRepository = seriesStatisticsRepository;
        }

        public List<SeriesStatistics> SeriesStatistics()
        {
            var seasonStatistics = _seriesStatisticsRepository.SeriesStatistics();

            return seasonStatistics.GroupBy(s => s.SeriesId).Select(s => MapSeriesStatistics(s.ToList())).ToList();
        }

        List<MovieStatistics> ISeriesStatisticsService.MovieStatistics()
        {
            // TODO: Hook up
            return new List<MovieStatistics>();
        }

        public SeriesStatistics SeriesStatistics(int seriesId)
        {
            var stats = _seriesStatisticsRepository.SeriesStatistics(seriesId);

            if (stats == null || stats.Count == 0)
            {
                return new SeriesStatistics();
            }

            return MapSeriesStatistics(stats);
        }

        MovieStatistics ISeriesStatisticsService.MovieStatistics(int movieId)
        {
            // TODO: Hook up
            return new MovieStatistics();
        }

        private SeriesStatistics MapSeriesStatistics(List<SeasonStatistics> seasonStatistics)
        {
            var seriesStatistics = new SeriesStatistics
                                   {
                                       SeasonStatistics = seasonStatistics,
                                       SeriesId = seasonStatistics.First().SeriesId,
                                       EpisodeFileCount = seasonStatistics.Sum(s => s.EpisodeFileCount),
                                       EpisodeCount = seasonStatistics.Sum(s => s.EpisodeCount),
                                       TotalEpisodeCount = seasonStatistics.Sum(s => s.TotalEpisodeCount),
                                       SizeOnDisk = seasonStatistics.Sum(s => s.SizeOnDisk),
                                       ReleaseGroups = seasonStatistics.SelectMany(s => s.ReleaseGroups).Distinct().ToList()
                                   };

            var nextAiring = seasonStatistics.Where(s => s.NextAiring != null).MinBy(s => s.NextAiring);
            var previousAiring = seasonStatistics.Where(s => s.PreviousAiring != null).MaxBy(s => s.PreviousAiring);

            seriesStatistics.NextAiringString = nextAiring?.NextAiringString;
            seriesStatistics.PreviousAiringString = previousAiring?.PreviousAiringString;

            return seriesStatistics;
        }
    }
}
