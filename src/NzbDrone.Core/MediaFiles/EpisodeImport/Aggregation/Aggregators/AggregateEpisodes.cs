using System.Collections.Generic;
using System.IO;
using NLog;
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

            if (bestEpisodeInfoForEpisodes == null)
            {
                return new List<Episode>();
            }

            var episodes = _parsingService.GetEpisodes(bestEpisodeInfoForEpisodes, localEpisode.Series, localEpisode.SceneSource);

            return episodes;
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
