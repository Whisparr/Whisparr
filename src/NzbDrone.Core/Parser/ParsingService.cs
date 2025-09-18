using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NLog.Fluent;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Instrumentation.Extensions;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Parser
{
    public interface IParsingService
    {
        Series GetSeries(string title);
        RemoteEpisode Map(ParsedEpisodeInfo parsedEpisodeInfo, int tvdbId, SearchCriteriaBase searchCriteria = null);
        RemoteEpisode Map(ParsedEpisodeInfo parsedEpisodeInfo, Series series);
        RemoteEpisode Map(ParsedEpisodeInfo parsedEpisodeInfo, int seriesId, IEnumerable<int> episodeIds);
        List<Episode> GetEpisodes(ParsedEpisodeInfo parsedEpisodeInfo, Series series, bool sceneSource, SearchCriteriaBase searchCriteria = null);
        ParsedEpisodeInfo ParseSpecialEpisodeTitle(ParsedEpisodeInfo parsedEpisodeInfo, string releaseTitle, int tvdbId, SearchCriteriaBase searchCriteria = null);
        ParsedEpisodeInfo ParseSpecialEpisodeTitle(ParsedEpisodeInfo parsedEpisodeInfo, string releaseTitle, Series series);
        RemoteEpisode TryMapByExternalId(string releaseTitle, ParsedEpisodeInfo parsedEpisodeInfo, SearchCriteriaBase searchCriteria = null);
    }

    public class ParsingService : IParsingService
    {
        private readonly IEpisodeService _episodeService;
        private readonly ISeriesService _seriesService;
        private readonly Logger _logger;

        public ParsingService(IEpisodeService episodeService,
                              ISeriesService seriesService,
                              Logger logger)
        {
            _episodeService = episodeService;
            _seriesService = seriesService;
            _logger = logger;
        }

        public Series GetSeries(string title)
        {
            var parsedEpisodeInfo = Parser.ParseTitle(title);

            if (parsedEpisodeInfo == null)
            {
                return _seriesService.FindByTitle(title);
            }

            var series = _seriesService.FindByTitle(parsedEpisodeInfo.SeriesTitle);

            if (series == null && parsedEpisodeInfo.SeriesTitleInfo.AllTitles != null)
            {
                series = GetSeriesByAllTitles(parsedEpisodeInfo);
            }

            if (series == null)
            {
                series = _seriesService.FindByTitle(parsedEpisodeInfo.SeriesTitleInfo.TitleWithoutYear,
                                                    parsedEpisodeInfo.SeriesTitleInfo.Year);
            }

            return series;
        }

        private Series GetSeriesByAllTitles(ParsedEpisodeInfo parsedEpisodeInfo)
        {
            Series foundSeries = null;
            int? foundTvdbId = null;

            // Match each title individually, they must all resolve to the same tvdbid
            foreach (var title in parsedEpisodeInfo.SeriesTitleInfo.AllTitles)
            {
                var series = _seriesService.FindByTitle(title);
                var tvdbId = series?.TvdbId;

                if (!tvdbId.HasValue)
                {
                    _logger.Trace("Title {0} not matching any series.", title);
                    continue;
                }

                if (foundTvdbId.HasValue && tvdbId != foundTvdbId)
                {
                    _logger.Trace("Title {0} both matches tvdbid {1} and {2}, no series selected.", parsedEpisodeInfo.SeriesTitle, foundTvdbId, tvdbId);
                    return null;
                }

                if (foundSeries == null)
                {
                    foundSeries = series;
                }

                foundTvdbId = tvdbId;
            }

            if (foundSeries == null && foundTvdbId.HasValue)
            {
                foundSeries = _seriesService.FindByTvdbId(foundTvdbId.Value);
            }

            return foundSeries;
        }

        public RemoteEpisode Map(ParsedEpisodeInfo parsedEpisodeInfo, int tvdbId, SearchCriteriaBase searchCriteria = null)
        {
            return Map(parsedEpisodeInfo, tvdbId, null, searchCriteria);
        }

        public RemoteEpisode Map(ParsedEpisodeInfo parsedEpisodeInfo, Series series)
        {
            return Map(parsedEpisodeInfo, 0, series, null);
        }

        public RemoteEpisode Map(ParsedEpisodeInfo parsedEpisodeInfo, int seriesId, IEnumerable<int> episodeIds)
        {
            return new RemoteEpisode
                   {
                       ParsedEpisodeInfo = parsedEpisodeInfo,
                       Series = _seriesService.GetSeries(seriesId),
                       Episodes = _episodeService.GetEpisodes(episodeIds)
                   };
        }

        private RemoteEpisode Map(ParsedEpisodeInfo parsedEpisodeInfo, int tvdbId, Series series, SearchCriteriaBase searchCriteria)
        {
            var remoteEpisode = new RemoteEpisode
            {
                ParsedEpisodeInfo = parsedEpisodeInfo
            };

            if (series == null)
            {
                var seriesMatch = FindSeries(parsedEpisodeInfo, tvdbId, searchCriteria);

                if (seriesMatch != null)
                {
                    series = seriesMatch.Series;
                    remoteEpisode.SeriesMatchType = seriesMatch.MatchType;
                }
            }

            if (series != null)
            {
                remoteEpisode.Series = series;

                remoteEpisode.Episodes = GetEpisodes(parsedEpisodeInfo, series, searchCriteria);
            }

            remoteEpisode.Languages = parsedEpisodeInfo.Languages;

            if (remoteEpisode.Episodes == null)
            {
                remoteEpisode.Episodes = new List<Episode>();
            }

            if (searchCriteria != null)
            {
                var requestedEpisodes = searchCriteria.Episodes.ToDictionaryIgnoreDuplicates(v => v.Id);
                remoteEpisode.EpisodeRequested = remoteEpisode.Episodes.Any(v => requestedEpisodes.ContainsKey(v.Id));
            }

            return remoteEpisode;
        }

        public List<Episode> GetEpisodes(ParsedEpisodeInfo parsedEpisodeInfo, Series series, bool sceneSource, SearchCriteriaBase searchCriteria = null)
        {
            if (sceneSource)
            {
                var remoteEpisode = Map(parsedEpisodeInfo, 0, series, searchCriteria);

                return remoteEpisode.Episodes;
            }

            return GetEpisodes(parsedEpisodeInfo, series, searchCriteria);
        }

        private List<Episode> GetEpisodes(ParsedEpisodeInfo parsedEpisodeInfo, Series series, SearchCriteriaBase searchCriteria)
        {
            var episodeInfo = GetDailyEpisode(series, parsedEpisodeInfo.AirDate, parsedEpisodeInfo.ReleaseTokens, searchCriteria);

            if (episodeInfo != null)
            {
                return new List<Episode> { episodeInfo };
            }

            return new List<Episode>();
        }

        public ParsedEpisodeInfo ParseSpecialEpisodeTitle(ParsedEpisodeInfo parsedEpisodeInfo, string releaseTitle, int tvdbId, SearchCriteriaBase searchCriteria = null)
        {
            if (searchCriteria != null)
            {
                if (tvdbId != 0 && tvdbId == searchCriteria.Series.TvdbId)
                {
                    return ParseSpecialEpisodeTitle(parsedEpisodeInfo, releaseTitle, searchCriteria.Series);
                }
            }

            var series = GetSeries(releaseTitle);

            if (series == null)
            {
                series = _seriesService.FindByTitleInexact(releaseTitle);
            }

            if (series == null && tvdbId > 0)
            {
                series = _seriesService.FindByTvdbId(tvdbId);
            }

            if (series == null)
            {
                _logger.Debug("No matching series {0}", releaseTitle);
                return null;
            }

            return ParseSpecialEpisodeTitle(parsedEpisodeInfo, releaseTitle, series);
        }

        public ParsedEpisodeInfo ParseSpecialEpisodeTitle(ParsedEpisodeInfo parsedEpisodeInfo, string releaseTitle, Series series)
        {
            // find special episode in series season 0
            var episode = _episodeService.FindEpisodeByTitle(series.Id, 0, releaseTitle);

            if (episode != null)
            {
                // create parsed info from tv episode
                var info = new ParsedEpisodeInfo
                {
                    ReleaseTitle = releaseTitle,
                    SeriesTitle = series.Title,
                    SeriesTitleInfo = new SeriesTitleInfo
                        {
                            Title = series.Title
                        },
                    Quality = QualityParser.ParseQuality(releaseTitle),
                    ReleaseGroup = Parser.ParseReleaseGroup(releaseTitle),
                    Languages = LanguageParser.ParseLanguages(releaseTitle)
                };

                _logger.Debug("Found special episode {0} for title '{1}'", info, releaseTitle);
                return info;
            }

            return null;
        }

        private FindSeriesResult FindSeries(ParsedEpisodeInfo parsedEpisodeInfo, int tvdbId, SearchCriteriaBase searchCriteria)
        {
            Series series = null;

            if (searchCriteria != null)
            {
                if (searchCriteria.Series.CleanTitle == parsedEpisodeInfo.SeriesTitle.CleanSeriesTitle())
                {
                    return new FindSeriesResult(searchCriteria.Series, SeriesMatchType.Title);
                }

                if (tvdbId > 0 && tvdbId == searchCriteria.Series.TvdbId)
                {
                    _logger.Debug()
                           .Message("Found matching series by TVDB ID {0}, an alias may be needed for: {1}", tvdbId, parsedEpisodeInfo.SeriesTitle)
                           .Property("TvdbId", tvdbId)
                           .Property("ParsedEpisodeInfo", parsedEpisodeInfo)
                           .WriteSentryWarn("TvdbIdMatch", tvdbId.ToString(), parsedEpisodeInfo.SeriesTitle)
                           .Write();

                    return new FindSeriesResult(searchCriteria.Series, SeriesMatchType.Id);
                }
            }

            var matchType = SeriesMatchType.Unknown;
            series = _seriesService.FindByTitle(parsedEpisodeInfo.SeriesTitle);

            if (series != null)
            {
                matchType = SeriesMatchType.Title;
            }

            if (series == null && parsedEpisodeInfo.SeriesTitleInfo.AllTitles != null)
            {
                series = GetSeriesByAllTitles(parsedEpisodeInfo);
                matchType = SeriesMatchType.Title;
            }

            if (series == null && parsedEpisodeInfo.SeriesTitleInfo.Year > 0)
            {
                series = _seriesService.FindByTitle(parsedEpisodeInfo.SeriesTitleInfo.TitleWithoutYear, parsedEpisodeInfo.SeriesTitleInfo.Year);
                matchType = SeriesMatchType.Title;
            }

            if (series == null)
            {
                series = _seriesService.FindByTitleSlug(parsedEpisodeInfo.SeriesTitle);
                matchType = SeriesMatchType.Title;
            }

            if (series == null && tvdbId > 0)
            {
                series = _seriesService.FindByTvdbId(tvdbId);

                if (series != null)
                {
                    _logger.Debug()
                           .Message("Found matching series by TVDB ID {0}, an alias may be needed for: {1}", tvdbId, parsedEpisodeInfo.SeriesTitle)
                           .Property("TvdbId", tvdbId)
                           .Property("ParsedEpisodeInfo", parsedEpisodeInfo)
                           .WriteSentryWarn("TvdbIdMatch", tvdbId.ToString(), parsedEpisodeInfo.SeriesTitle)
                           .Write();

                    matchType = SeriesMatchType.Id;
                }
            }

            if (series == null)
            {
                _logger.Debug("No matching series {0}", parsedEpisodeInfo.SeriesTitle);
                return null;
            }

            return new FindSeriesResult(series, matchType);
        }

        private Episode GetDailyEpisode(Series series, string airDate, string part, SearchCriteriaBase searchCriteria)
        {
            Episode episodeInfo = null;

            if (searchCriteria != null)
            {
                episodeInfo = searchCriteria.Episodes.SingleOrDefault(
                    e => e.AirDate == airDate);
            }

            if (episodeInfo == null)
            {
                episodeInfo = _episodeService.FindEpisode(series.Id, airDate, part);
            }
            else
            {
                // Check if there are multiple episodes for this date in the database to determine if validation is needed
                // This avoids unnecessary validation calls for single-episode dates
                var allEpisodesForDate = _episodeService.GetEpisodeBySeries(series.Id)
                    .Where(e => e.AirDate == airDate).ToList();

                // For single-episode dates, trust the search criteria episode without additional validation
                if (allEpisodesForDate.Count <= 1)
                {
                    return episodeInfo;
                }

                // Multiple episodes exist for this date - validate the release matches the correct episode
                var validatedEpisode = _episodeService.FindEpisode(series.Id, airDate, part);
                if (validatedEpisode == null)
                {
                    _logger.Debug("Release does not match any specific episode for multi-episode date {0}. Release: {1}. Rejecting to prevent incorrect download.", airDate, part);
                    return null;
                }

                // If the validated episode doesn't match the search criteria episode, reject
                if (validatedEpisode.Id != episodeInfo.Id)
                {
                    _logger.Debug("Release matches a different episode than expected for date {0}. Expected: {1} ({2}), Found: {3} ({4}). Rejecting to prevent incorrect download.",
                        airDate,
                        episodeInfo.Id,
                        episodeInfo.Title,
                        validatedEpisode.Id,
                        validatedEpisode.Title);
                    return null;
                }

                episodeInfo = validatedEpisode;
            }

            return episodeInfo;
        }

        public RemoteEpisode TryMapByExternalId(string releaseTitle, ParsedEpisodeInfo parsedEpisodeInfo, SearchCriteriaBase searchCriteria = null)
        {
            if (string.IsNullOrWhiteSpace(releaseTitle))
            {
                return null;
            }

            string extractedExternalId;
            try
            {
                // Extract External ID from the release title
                extractedExternalId = ExternalIdParser.ExtractExternalId(releaseTitle);
                if (string.IsNullOrWhiteSpace(extractedExternalId))
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                // Gracefully handle any issues with External ID parsing (e.g., in test environments)
                _logger.Debug(ex, "Failed to extract External ID from release title: {0}", releaseTitle);
                return null;
            }

            // Create ParsedEpisodeInfo if not provided
            if (parsedEpisodeInfo == null)
            {
                parsedEpisodeInfo = new ParsedEpisodeInfo
                {
                    ReleaseTitle = releaseTitle,
                    Languages = LanguageParser.ParseLanguages(releaseTitle),
                    Quality = QualityParser.ParseQuality(releaseTitle),
                    ReleaseGroup = Parser.ParseReleaseGroup(releaseTitle)
                };
            }

            // If we have search criteria with a specific External ID, verify it matches
            if (searchCriteria is SingleEpisodeSearchCriteria singleEpisodeSearchCriteria &&
                !string.IsNullOrWhiteSpace(singleEpisodeSearchCriteria.ExternalId))
            {
                if (!string.Equals(extractedExternalId, singleEpisodeSearchCriteria.ExternalId, System.StringComparison.OrdinalIgnoreCase))
                {
                    return null;
                }
            }

            Episode episode;

            try
            {
                // Find the episode by External ID
                episode = _episodeService.FindEpisodeByExternalId(extractedExternalId);
                if (episode == null)
                {
                    return null;
                }

                // Episode should have series information - if not, skip
                if (episode.Series == null && episode.SeriesId == 0)
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                // Gracefully handle database access issues (e.g., in test environments)
                _logger.Debug(ex, "Failed to lookup episode by External ID: {0}", extractedExternalId);
                return null;
            }

            // Update ParsedEpisodeInfo with episode information for proper validation
            if (parsedEpisodeInfo.AirDate.IsNullOrWhiteSpace() && !episode.AirDate.IsNullOrWhiteSpace())
            {
                parsedEpisodeInfo.AirDate = episode.AirDate;
            }

            // Create a RemoteEpisode with the matched episode
            var remoteEpisode = new RemoteEpisode
            {
                ParsedEpisodeInfo = parsedEpisodeInfo,
                Episodes = new List<Episode> { episode },
                Series = episode.Series,
                Languages = parsedEpisodeInfo.Languages,
                SeriesMatchType = SeriesMatchType.ExternalId
            };

            // Mark as episode requested if this is from search criteria
            if (searchCriteria != null)
            {
                var requestedEpisodes = searchCriteria.Episodes.ToDictionaryIgnoreDuplicates(v => v.Id);
                remoteEpisode.EpisodeRequested = remoteEpisode.Episodes.Any(v => requestedEpisodes.ContainsKey(v.Id));
            }

            return remoteEpisode;
        }
    }
}
