using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Instrumentation.Extensions;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.Download.Aggregation;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.DecisionEngine
{
    public interface IMakeDownloadDecision
    {
        List<DownloadDecision> GetRssDecision(List<ReleaseInfo> reports, bool pushedRelease = false);
        List<DownloadDecision> GetSearchDecision(List<ReleaseInfo> reports, SearchCriteriaBase searchCriteriaBase);
    }

    public class DownloadDecisionMaker : IMakeDownloadDecision
    {
        private readonly IEnumerable<IDecisionEngineSpecification> _specifications;
        private readonly IParsingService _parsingService;
        private readonly ICustomFormatCalculationService _formatCalculator;
        private readonly IRemoteEpisodeAggregationService _aggregationService;
        private readonly IEpisodeService _episodeService;
        private readonly ISeriesService _seriesService;
        private readonly Logger _logger;

        public DownloadDecisionMaker(IEnumerable<IDecisionEngineSpecification> specifications,
                                     IParsingService parsingService,
                                     ICustomFormatCalculationService formatService,
                                     IRemoteEpisodeAggregationService aggregationService,
                                     IEpisodeService episodeService,
                                     ISeriesService seriesService,
                                     Logger logger)
        {
            _specifications = specifications;
            _parsingService = parsingService;
            _formatCalculator = formatService;
            _aggregationService = aggregationService;
            _episodeService = episodeService;
            _seriesService = seriesService;
            _logger = logger;
        }

        public List<DownloadDecision> GetRssDecision(List<ReleaseInfo> reports, bool pushedRelease = false)
        {
            return GetDecisions(reports, pushedRelease).ToList();
        }

        public List<DownloadDecision> GetSearchDecision(List<ReleaseInfo> reports, SearchCriteriaBase searchCriteriaBase)
        {
            return GetDecisions(reports, false, searchCriteriaBase).ToList();
        }

        private IEnumerable<DownloadDecision> GetDecisions(List<ReleaseInfo> reports, bool pushedRelease, SearchCriteriaBase searchCriteria = null)
        {
            if (reports.Any())
            {
                _logger.ProgressInfo("Processing {0} releases", reports.Count);
            }
            else
            {
                _logger.ProgressInfo("No results found");
            }

            var reportNumber = 1;

            foreach (var report in reports)
            {
                DownloadDecision decision = null;
                _logger.ProgressTrace("Processing release {0}/{1}", reportNumber, reports.Count);
                _logger.Debug("Processing release '{0}' from '{1}'", report.Title, report.Indexer);

                try
                {
                    var parsedEpisodeInfo = Parser.Parser.ParseTitle(report.Title);

                    // Try standard parsing first
                    if (parsedEpisodeInfo != null && !parsedEpisodeInfo.SeriesTitle.IsNullOrWhiteSpace())
                    {
                        var remoteEpisode = _parsingService.Map(parsedEpisodeInfo, report.TvdbId, searchCriteria);
                        remoteEpisode.Release = report;

                        if (remoteEpisode.Series != null && !remoteEpisode.Episodes.Empty())
                        {
                            // Standard parsing succeeded
                            _aggregationService.Augment(remoteEpisode);
                            remoteEpisode.CustomFormats = _formatCalculator.ParseCustomFormat(remoteEpisode, remoteEpisode.Release.Size);
                            remoteEpisode.CustomFormatScore = remoteEpisode?.Series?.QualityProfile?.Value.CalculateCustomFormatScore(remoteEpisode.CustomFormats) ?? 0;
                            remoteEpisode.DownloadAllowed = remoteEpisode.Episodes.Any();
                            decision = GetDecisionForReport(remoteEpisode, searchCriteria);
                        }
                    }

                    // If standard parsing failed at any point, try External ID parsing
                    if (decision == null)
                    {
                        // Ensure we have basic parsed info for External ID parsing
                        if (parsedEpisodeInfo == null)
                        {
                            parsedEpisodeInfo = new ParsedEpisodeInfo
                            {
                                Languages = LanguageParser.ParseLanguages(report.Title),
                                Quality = QualityParser.ParseQuality(report.Title)
                            };
                        }

                        var externalIdBasedDecision = TryParseByExternalId(report, parsedEpisodeInfo, searchCriteria);
                        if (externalIdBasedDecision != null)
                        {
                            decision = externalIdBasedDecision;
                        }
                        else if (searchCriteria != null)
                        {
                            // Both standard and External ID parsing failed during search - provide rejection with reason
                            var remoteEpisode = new RemoteEpisode
                            {
                                Release = report,
                                ParsedEpisodeInfo = parsedEpisodeInfo,
                                Languages = parsedEpisodeInfo.Languages
                            };

                            // Try to provide more specific error message based on what we attempted
                            string rejectionReason;
                            if (parsedEpisodeInfo.SeriesTitle.IsNullOrWhiteSpace())
                            {
                                rejectionReason = "Unable to parse release";
                            }
                            else
                            {
                                // We had a series title but mapping failed, check if we tried standard parsing
                                var standardRemoteEpisode = _parsingService.Map(parsedEpisodeInfo, report.TvdbId, searchCriteria);
                                if (standardRemoteEpisode.Series == null)
                                {
                                    rejectionReason = "Unknown Series";
                                }
                                else
                                {
                                    rejectionReason = "Unable to identify correct episode(s) using release name and scene mappings";
                                }
                            }

                            decision = new DownloadDecision(remoteEpisode, new Rejection(rejectionReason));
                        }
                        // For RSS feeds, if both parsing methods fail, return no decision (null)
                    }
                }
                catch (Exception e)
                {
                    _logger.Error(e, "Couldn't process release.");

                    var remoteEpisode = new RemoteEpisode { Release = report };
                    decision = new DownloadDecision(remoteEpisode, new Rejection("Unexpected error processing release"));
                }

                reportNumber++;

                if (decision != null)
                {
                    var source = pushedRelease ? ReleaseSourceType.ReleasePush : ReleaseSourceType.Rss;

                    if (searchCriteria != null)
                    {
                        if (searchCriteria.InteractiveSearch)
                        {
                            source = ReleaseSourceType.InteractiveSearch;
                        }
                        else if (searchCriteria.UserInvokedSearch)
                        {
                            source = ReleaseSourceType.UserInvokedSearch;
                        }
                        else
                        {
                            source = ReleaseSourceType.Search;
                        }
                    }

                    decision.RemoteEpisode.ReleaseSource = source;

                    if (decision.Rejections.Any())
                    {
                        _logger.Debug("Release '{0}' from '{1}' rejected for the following reasons: {2}", report.Title, report.Indexer, string.Join(", ", decision.Rejections));
                    }
                    else
                    {
                        _logger.Debug("Release '{0}' from '{1}' accepted", report.Title, report.Indexer);
                    }

                    yield return decision;
                }
            }
        }

        private DownloadDecision GetDecisionForReport(RemoteEpisode remoteEpisode, SearchCriteriaBase searchCriteria = null)
        {
            var reasons = Array.Empty<Rejection>();

            foreach (var specifications in _specifications.GroupBy(v => v.Priority).OrderBy(v => v.Key))
            {
                reasons = specifications.Select(c => EvaluateSpec(c, remoteEpisode, searchCriteria))
                                        .Where(c => c != null)
                                        .ToArray();

                if (reasons.Any())
                {
                    break;
                }
            }

            return new DownloadDecision(remoteEpisode, reasons.ToArray());
        }

        private Rejection EvaluateSpec(IDecisionEngineSpecification spec, RemoteEpisode remoteEpisode, SearchCriteriaBase searchCriteriaBase = null)
        {
            try
            {
                var result = spec.IsSatisfiedBy(remoteEpisode, searchCriteriaBase);

                if (!result.Accepted)
                {
                    return new Rejection(result.Reason, spec.Type);
                }
            }
            catch (Exception e)
            {
                e.Data.Add("report", remoteEpisode.Release.ToJson());
                e.Data.Add("parsed", remoteEpisode.ParsedEpisodeInfo.ToJson());
                _logger.Error(e, "Couldn't evaluate decision on {0}", remoteEpisode.Release.Title);
                return new Rejection($"{spec.GetType().Name}: {e.Message}");
            }

            return null;
        }

        private DownloadDecision TryParseByExternalId(ReleaseInfo report, ParsedEpisodeInfo parsedEpisodeInfo, SearchCriteriaBase searchCriteria)
        {
            // Extract External ID from the release title
            var extractedExternalId = ExternalIdParser.ExtractExternalId(report.Title);
            if (string.IsNullOrWhiteSpace(extractedExternalId))
            {
                return null;
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

            // Find the episode by External ID
            var episode = _episodeService.FindEpisodeByExternalId(extractedExternalId);
            if (episode == null)
            {
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
                Release = report,
                ParsedEpisodeInfo = parsedEpisodeInfo,
                Episodes = new List<Episode> { episode },
                Series = episode.Series ?? searchCriteria.Series,
                Languages = parsedEpisodeInfo.Languages,
                SeriesMatchType = SeriesMatchType.ExternalId
            };

            // If we don't have the series loaded, get it from the service
            if (remoteEpisode.Series == null && episode.SeriesId > 0)
            {
                remoteEpisode.Series = _seriesService.GetSeries(episode.SeriesId);
            }

            // As a fallback, try to use search criteria series if available
            if (remoteEpisode.Series == null && searchCriteria?.Series != null)
            {
                remoteEpisode.Series = searchCriteria.Series;
            }

            if (remoteEpisode.Series == null)
            {
                return new DownloadDecision(remoteEpisode, new Rejection("Unknown Series"));
            }

            // Mark as episode requested if this is from search criteria
            remoteEpisode.EpisodeRequested = searchCriteria != null;

            // Augment and format the remote episode
            _aggregationService.Augment(remoteEpisode);
            remoteEpisode.CustomFormats = _formatCalculator.ParseCustomFormat(remoteEpisode, remoteEpisode.Release.Size);
            remoteEpisode.CustomFormatScore = remoteEpisode?.Series?.QualityProfile?.Value.CalculateCustomFormatScore(remoteEpisode.CustomFormats) ?? 0;
            remoteEpisode.DownloadAllowed = remoteEpisode.Episodes.Any();

            return GetDecisionForReport(remoteEpisode, searchCriteria);
        }
    }
}
