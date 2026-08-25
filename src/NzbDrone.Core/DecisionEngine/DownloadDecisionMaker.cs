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

namespace NzbDrone.Core.DecisionEngine
{
    public interface IMakeDownloadDecision
    {
        List<DownloadDecision> GetRssDecision(List<ReleaseInfo> reports, bool pushedRelease = false);
        List<DownloadDecision> GetSearchDecision(List<ReleaseInfo> reports, SearchCriteriaBase searchCriteriaBase);
    }

    public class DownloadDecisionMaker : IMakeDownloadDecision
    {
        private readonly IEnumerable<IDownloadDecisionEngineSpecification> _specifications;
        private readonly IParsingService _parsingService;
        private readonly ICustomFormatCalculationService _formatCalculator;
        private readonly IRemoteEpisodeAggregationService _aggregationService;
        private readonly Logger _logger;

        public DownloadDecisionMaker(IEnumerable<IDownloadDecisionEngineSpecification> specifications,
                                     IParsingService parsingService,
                                     ICustomFormatCalculationService formatService,
                                     IRemoteEpisodeAggregationService aggregationService,
                                     Logger logger)
        {
            _specifications = specifications;
            _parsingService = parsingService;
            _formatCalculator = formatService;
            _aggregationService = aggregationService;
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

                    // Try to extract external ID from the release title
                    var releaseExternalId = Parser.Parser.ParseExternalId(report.Title);

                    // If ParseTitle failed but we have an external ID, create a basic ParsedEpisodeInfo
                    if (parsedEpisodeInfo == null && !releaseExternalId.IsNullOrWhiteSpace())
                    {
                        parsedEpisodeInfo = new ParsedEpisodeInfo
                        {
                            ReleaseTitle = report.Title,
                            Languages = LanguageParser.ParseLanguages(report.Title),
                            Quality = QualityParser.ParseQuality(report.Title)
                        };
                    }

                    // Store the external ID if we have parsedEpisodeInfo
                    if (parsedEpisodeInfo != null)
                    {
                        parsedEpisodeInfo.ExternalId = releaseExternalId;
                    }

                    // Try to map if we have a series title OR an external ID
                    if (parsedEpisodeInfo != null && (!parsedEpisodeInfo.SeriesTitle.IsNullOrWhiteSpace() || !releaseExternalId.IsNullOrWhiteSpace()))
                    {
                        var remoteEpisode = _parsingService.Map(parsedEpisodeInfo, report.TvdbId, searchCriteria);
                        remoteEpisode.Release = report;

                        if (remoteEpisode.Series == null)
                        {
                            decision = new DownloadDecision(remoteEpisode, new DownloadRejection(DownloadRejectionReason.UnknownSeries, "Unknown Series"));
                        }
                        else if (remoteEpisode.Episodes.Empty())
                        {
                            decision = new DownloadDecision(remoteEpisode, new DownloadRejection(DownloadRejectionReason.UnknownEpisode, "Unable to identify correct episode(s) using release name and scene mappings"));
                        }
                        else
                        {
                            _aggregationService.Augment(remoteEpisode);

                            remoteEpisode.CustomFormats = _formatCalculator.ParseCustomFormat(remoteEpisode, remoteEpisode.Release.Size);
                            remoteEpisode.CustomFormatScore = remoteEpisode?.Series?.QualityProfile?.Value.CalculateCustomFormatScore(remoteEpisode.CustomFormats) ?? 0;

                            _logger.Trace("Custom Format Score of '{0}' [{1}] calculated for '{2}'", remoteEpisode.CustomFormatScore, remoteEpisode.CustomFormats?.ConcatToString(), report.Title);

                            remoteEpisode.DownloadAllowed = remoteEpisode.Episodes.Any();
                            decision = GetDecisionForReport(remoteEpisode, searchCriteria);
                        }
                    }
                    else if (decision == null)
                    {
                        // Neither title parsing nor external ID available
                        var remoteEpisode = new RemoteEpisode
                        {
                            Release = report,
                            ParsedEpisodeInfo = parsedEpisodeInfo ?? new ParsedEpisodeInfo
                            {
                                ReleaseTitle = report.Title,
                                Languages = LanguageParser.ParseLanguages(report.Title),
                                Quality = QualityParser.ParseQuality(report.Title)
                            },
                            Languages = parsedEpisodeInfo?.Languages ?? LanguageParser.ParseLanguages(report.Title)
                        };

                        decision = new DownloadDecision(remoteEpisode, new DownloadRejection(DownloadRejectionReason.UnableToParse, "Unable to parse release"));
                    }
                }
                catch (Exception e)
                {
                    _logger.Error(e, "Couldn't process release.");

                    var remoteEpisode = new RemoteEpisode { Release = report };
                    decision = new DownloadDecision(remoteEpisode, new DownloadRejection(DownloadRejectionReason.Error, "Unexpected error processing release"));
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
            var reasons = Array.Empty<DownloadRejection>();

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

        private DownloadRejection EvaluateSpec(IDownloadDecisionEngineSpecification spec, RemoteEpisode remoteEpisode, SearchCriteriaBase searchCriteriaBase = null)
        {
            try
            {
                var result = spec.IsSatisfiedBy(remoteEpisode, searchCriteriaBase);

                if (!result.Accepted)
                {
                    return new DownloadRejection(result.Reason, result.Message, spec.Type);
                }
            }
            catch (Exception e)
            {
                e.Data.Add("report", remoteEpisode.Release.ToJson());
                e.Data.Add("parsed", remoteEpisode.ParsedEpisodeInfo.ToJson());
                _logger.Error(e, "Couldn't evaluate decision on {0}", remoteEpisode.Release.Title);
                return new DownloadRejection(DownloadRejectionReason.DecisionError, $"{spec.GetType().Name}: {e.Message}");
            }

            return null;
        }
    }
}
