using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Download;
using NzbDrone.Core.Download.TrackedDownloads;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.MediaFiles.EpisodeImport.Aggregation.Aggregators
{
    public class AggregateEpisodes : IAggregateLocalEpisode
    {
        private readonly IParsingService _parsingService;
        private readonly ITrackedDownloadService _trackedDownloadService;
        private readonly Logger _logger;

        public AggregateEpisodes(IParsingService parsingService, ITrackedDownloadService trackedDownloadService, Logger logger)
        {
            _parsingService = parsingService;
            _trackedDownloadService = trackedDownloadService;
            _logger = logger;
        }

        public LocalEpisode Aggregate(LocalEpisode localEpisode, DownloadClientItem downloadClientItem)
        {
            localEpisode.Episodes = GetEpisodes(localEpisode);

            return localEpisode;
        }

        private ParsedEpisodeInfo GetBestEpisodeInfo(LocalEpisode localEpisode)
        {
            var parsedEpisodeInfo = localEpisode.FileEpisodeInfo;
            var downloadClientEpisodeInfo = localEpisode.DownloadClientEpisodeInfo;
            var folderEpisodeInfo = localEpisode.FolderEpisodeInfo;

            if (!localEpisode.OtherVideoFiles && !SceneChecker.IsSceneTitle(Path.GetFileNameWithoutExtension(localEpisode.Path)))
            {
                if (downloadClientEpisodeInfo != null &&
                    PreferOtherEpisodeInfo(parsedEpisodeInfo, downloadClientEpisodeInfo))
                {
                    parsedEpisodeInfo = localEpisode.DownloadClientEpisodeInfo;
                }
                else if (folderEpisodeInfo != null &&
                         PreferOtherEpisodeInfo(parsedEpisodeInfo, folderEpisodeInfo))
                {
                    parsedEpisodeInfo = localEpisode.FolderEpisodeInfo;
                }
            }

            if (parsedEpisodeInfo == null)
            {
                parsedEpisodeInfo = GetSpecialEpisodeInfo(localEpisode, parsedEpisodeInfo);
            }

            return parsedEpisodeInfo;
        }

        private ParsedEpisodeInfo GetSpecialEpisodeInfo(LocalEpisode localEpisode, ParsedEpisodeInfo parsedEpisodeInfo)
        {
            var title = Path.GetFileNameWithoutExtension(localEpisode.Path);
            var specialEpisodeInfo = _parsingService.ParseSpecialEpisodeTitle(parsedEpisodeInfo, title, localEpisode.Series);

            return specialEpisodeInfo;
        }

        private List<Episode> GetEpisodes(LocalEpisode localEpisode)
        {
            // Check if this is a force download that should bypass normal episode parsing
            if (IsForceDownload(localEpisode))
            {
                var trackedDownload = _trackedDownloadService.Find(localEpisode.DownloadItem.DownloadId);

                // For force downloads, ensure we have proper FileEpisodeInfo to prevent other aggregators from failing
                if (localEpisode.FileEpisodeInfo == null)
                {
                    localEpisode.FileEpisodeInfo = trackedDownload.RemoteEpisode.ParsedEpisodeInfo ?? new ParsedEpisodeInfo
                    {
                        Quality = trackedDownload.RemoteEpisode.ParsedEpisodeInfo?.Quality ?? new QualityModel(),
                        Languages = trackedDownload.RemoteEpisode.Languages
                    };
                }

                // Return the explicitly specified episodes, bypassing all parsing logic
                return trackedDownload.RemoteEpisode.Episodes;
            }

            var bestEpisodeInfoForEpisodes = GetBestEpisodeInfo(localEpisode);
            var isMediaFile = MediaFileExtensions.Extensions.Contains(Path.GetExtension(localEpisode.Path));

            // If normal parsing fails, try to match by external ID from the filename
            if (bestEpisodeInfoForEpisodes == null)
            {
                var episodes = TryMatchByExternalId(localEpisode);
                if (episodes != null && episodes.Any())
                {
                    return episodes;
                }

                return new List<Episode>();
            }

            var parsedEpisodes = _parsingService.GetEpisodes(bestEpisodeInfoForEpisodes, localEpisode.Series, localEpisode.SceneSource);

            return parsedEpisodes;
        }

        private List<Episode> TryMatchByExternalId(LocalEpisode localEpisode)
        {
            // Try to extract external ID from the filename
            var filename = Path.GetFileName(localEpisode.Path);
            var filenameExternalId = Parser.Parser.ParseExternalIdFromFilename(filename);

            if (filenameExternalId.IsNullOrWhiteSpace())
            {
                return null;
            }

            _logger.Debug("Attempting to match file by external ID: {0}", filenameExternalId);

            // First, check if we have a tracked download with matching episode info
            if (localEpisode.DownloadItem != null)
            {
                var trackedDownload = _trackedDownloadService.Find(localEpisode.DownloadItem.DownloadId);

                if (trackedDownload?.RemoteEpisode?.Episodes != null && trackedDownload.RemoteEpisode.Episodes.Any())
                {
                    // Check if the tracked download's episode has a matching external ID
                    var trackedEpisode = trackedDownload.RemoteEpisode.Episodes.FirstOrDefault();
                    if (trackedEpisode != null &&
                        !trackedEpisode.ExternalId.IsNullOrWhiteSpace() &&
                        string.Equals(trackedEpisode.ExternalId, filenameExternalId, System.StringComparison.OrdinalIgnoreCase))
                    {
                        _logger.Debug("Matched file to episode via external ID from tracked download: {0}", filenameExternalId);

                        // Update FileEpisodeInfo from the tracked download to help other aggregators
                        if (localEpisode.FileEpisodeInfo == null)
                        {
                            localEpisode.FileEpisodeInfo = trackedDownload.RemoteEpisode.ParsedEpisodeInfo ?? new ParsedEpisodeInfo
                            {
                                Quality = trackedDownload.RemoteEpisode.ParsedEpisodeInfo?.Quality ?? new QualityModel(),
                                Languages = trackedDownload.RemoteEpisode.Languages,
                                ExternalId = filenameExternalId
                            };
                        }

                        return trackedDownload.RemoteEpisode.Episodes;
                    }
                }
            }

            return null;
        }

        private bool PreferOtherEpisodeInfo(ParsedEpisodeInfo fileEpisodeInfo, ParsedEpisodeInfo otherEpisodeInfo)
        {
            if (fileEpisodeInfo == null)
            {
                return true;
            }

            return true;
        }

        private bool IsForceDownload(LocalEpisode localEpisode)
        {
            if (localEpisode.DownloadItem != null)
            {
                var trackedDownload = _trackedDownloadService.Find(localEpisode.DownloadItem.DownloadId);
                return trackedDownload?.RemoteEpisode?.ShouldOverride == true;
            }

            return false;
        }
    }
}
