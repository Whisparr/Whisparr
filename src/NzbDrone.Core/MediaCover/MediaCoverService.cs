using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using NLog;
using NzbDrone.Common;
using NzbDrone.Common.Disk;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Movies.Events;
using NzbDrone.Core.Movies.Performers;
using NzbDrone.Core.Movies.Performers.Events;
using NzbDrone.Core.Movies.Studios;
using NzbDrone.Core.Movies.Studios.Events;

namespace NzbDrone.Core.MediaCover
{
    public interface IMapCoversToLocal
    {
        Dictionary<string, FileInfo> GetMovieCoverFileInfos();
        Dictionary<string, FileInfo> GetPerformerCoverFileInfos();
        Dictionary<string, FileInfo> GetStudioCoverFileInfos();
        void ConvertToLocalUrls(int movieId, IEnumerable<MediaCover> covers, Dictionary<string, FileInfo> fileInfos = null);
        void ConvertToLocalPerformerUrls(int performerId, IEnumerable<MediaCover> covers, Dictionary<string, FileInfo> fileInfos = null);
        void ConvertToLocalStudioUrls(int studioId, IEnumerable<MediaCover> covers, Dictionary<string, FileInfo> fileInfos = null);
        void ConvertToLocalUrls(IEnumerable<Tuple<int, IEnumerable<MediaCover>>> items, Dictionary<string, FileInfo> coverFileInfos);
        void ConvertToLocalPerformerUrls(IEnumerable<Tuple<int, IEnumerable<MediaCover>>> items, Dictionary<string, FileInfo> coverFileInfos);
        void ConvertToLocalStudioUrls(IEnumerable<Tuple<int, IEnumerable<MediaCover>>> items, Dictionary<string, FileInfo> coverFileInfos);
        string GetMovieCoverPath(int movieId, MediaCoverTypes coverType, int? height = null);
        string GetPerformerCoverPath(int performerId, MediaCoverTypes coverType, int? height = null);
        string GetStudioCoverPath(int studioId, MediaCoverTypes coverType, int? height = null);
    }

    public class MediaCoverService :
        IHandleAsync<MovieUpdatedEvent>,
        IHandleAsync<PerformerUpdatedEvent>,
        IHandleAsync<StudioUpdatedEvent>,
        IHandleAsync<MoviesDeletedEvent>,
        IMapCoversToLocal
    {
        private readonly IMediaCoverProxy _mediaCoverProxy;
        private readonly IImageResizer _resizer;
        private readonly IHttpClient _httpClient;
        private readonly IDiskProvider _diskProvider;
        private readonly ICoverExistsSpecification _coverExistsSpecification;
        private readonly IConfigFileProvider _configFileProvider;
        private readonly IEventAggregator _eventAggregator;
        private readonly Logger _logger;

        private readonly string _coverRootFolder;

        // ImageSharp is slow on ARM (no hardware acceleration on mono yet)
        // So limit the number of concurrent resizing tasks
        private static SemaphoreSlim _semaphore = new SemaphoreSlim((int)Math.Ceiling(Environment.ProcessorCount / 2.0));

        public MediaCoverService(IMediaCoverProxy mediaCoverProxy,
                                 IImageResizer resizer,
                                 IHttpClient httpClient,
                                 IDiskProvider diskProvider,
                                 IAppFolderInfo appFolderInfo,
                                 ICoverExistsSpecification coverExistsSpecification,
                                 IConfigFileProvider configFileProvider,
                                 IEventAggregator eventAggregator,
                                 Logger logger)
        {
            _mediaCoverProxy = mediaCoverProxy;
            _resizer = resizer;
            _httpClient = httpClient;
            _diskProvider = diskProvider;
            _coverExistsSpecification = coverExistsSpecification;
            _configFileProvider = configFileProvider;
            _eventAggregator = eventAggregator;
            _logger = logger;

            _coverRootFolder = appFolderInfo.GetMediaCoverPath();
        }

        public string GetMovieCoverPath(int movieId, MediaCoverTypes coverType, int? height = null)
        {
            var heightSuffix = height.HasValue ? "-" + height.ToString() : "";

            return Path.Combine(GetMovieCoverPath(movieId), coverType.ToString().ToLower() + heightSuffix + GetExtension(coverType));
        }

        public string GetPerformerCoverPath(int performerId, MediaCoverTypes coverType, int? height = null)
        {
            var heightSuffix = height.HasValue ? "-" + height.ToString() : "";

            return Path.Combine(GetPerformerCoverPath(performerId), coverType.ToString().ToLower() + heightSuffix + GetExtension(coverType));
        }

        public string GetStudioCoverPath(int studioId, MediaCoverTypes coverType, int? height = null)
        {
            var heightSuffix = height.HasValue ? "-" + height.ToString() : "";

            return Path.Combine(GetStudioCoverPath(studioId), coverType.ToString().ToLower() + heightSuffix + GetExtension(coverType));
        }

        public Dictionary<string, FileInfo> GetMovieCoverFileInfos()
        {
            return GetCoverFileInfos("movie");
        }

        public Dictionary<string, FileInfo> GetPerformerCoverFileInfos()
        {
            return GetCoverFileInfos("performer");
        }

        public Dictionary<string, FileInfo> GetStudioCoverFileInfos()
        {
            return GetCoverFileInfos("studio");
        }

        public void ConvertToLocalUrls(int movieId, IEnumerable<MediaCover> covers, Dictionary<string, FileInfo> fileInfos = null)
        {
            if (movieId == 0)
            {
                // Movie isn't in Whisparr yet, map via a proxy to circument referrer issues
                foreach (var mediaCover in covers)
                {
                    mediaCover.Url = _mediaCoverProxy.RegisterUrl(mediaCover.RemoteUrl);
                }
            }
            else
            {
                foreach (var mediaCover in covers)
                {
                    if (mediaCover.CoverType == MediaCoverTypes.Unknown)
                    {
                        continue;
                    }

                    var filePath = GetMovieCoverPath(movieId, mediaCover.CoverType);

                    mediaCover.Url = _configFileProvider.UrlBase + @"/MediaCover/movie/" + movieId + "/" + mediaCover.CoverType.ToString().ToLower() + GetExtension(mediaCover.CoverType);

                    FileInfo file;
                    var fileExists = false;
                    if (fileInfos != null)
                    {
                        fileExists = fileInfos.TryGetValue(filePath, out file);
                    }
                    else
                    {
                        file = _diskProvider.GetFileInfo(filePath);
                        fileExists = file.Exists;
                    }

                    if (fileExists)
                    {
                        var lastWrite = file.LastWriteTimeUtc;
                        mediaCover.Url += "?lastWrite=" + lastWrite.Ticks;
                    }
                }
            }
        }

        public void ConvertToLocalPerformerUrls(int performerId, IEnumerable<MediaCover> covers, Dictionary<string, FileInfo> fileInfos = null)
        {
            if (performerId == 0)
            {
                // Movie isn't in Whisparr yet, map via a proxy to circument referrer issues
                foreach (var mediaCover in covers)
                {
                    mediaCover.Url = _mediaCoverProxy.RegisterUrl(mediaCover.RemoteUrl);
                }
            }
            else
            {
                foreach (var mediaCover in covers)
                {
                    if (mediaCover.CoverType == MediaCoverTypes.Unknown)
                    {
                        continue;
                    }

                    var filePath = GetPerformerCoverPath(performerId, mediaCover.CoverType);

                    mediaCover.Url = _configFileProvider.UrlBase + @"/MediaCover/performer/" + performerId + "/" + mediaCover.CoverType.ToString().ToLower() + GetExtension(mediaCover.CoverType);

                    FileInfo file;
                    var fileExists = false;
                    if (fileInfos != null)
                    {
                        fileExists = fileInfos.TryGetValue(filePath, out file);
                    }
                    else
                    {
                        file = _diskProvider.GetFileInfo(filePath);
                        fileExists = file.Exists;
                    }

                    if (fileExists)
                    {
                        var lastWrite = file.LastWriteTimeUtc;
                        mediaCover.Url += "?lastWrite=" + lastWrite.Ticks;
                    }
                }
            }
        }

        public void ConvertToLocalStudioUrls(int studioId, IEnumerable<MediaCover> covers, Dictionary<string, FileInfo> fileInfos = null)
        {
            if (studioId == 0)
            {
                // Movie isn't in Whisparr yet, map via a proxy to circument referrer issues
                foreach (var mediaCover in covers)
                {
                    mediaCover.Url = _mediaCoverProxy.RegisterUrl(mediaCover.RemoteUrl);
                }
            }
            else
            {
                foreach (var mediaCover in covers)
                {
                    if (mediaCover.CoverType == MediaCoverTypes.Unknown)
                    {
                        continue;
                    }

                    var filePath = GetStudioCoverPath(studioId);
                    var extension = GetExtension(mediaCover.CoverType);

                    // get files in studio folder
                    var pathExists = _diskProvider.FolderExists(filePath);
                    if (!pathExists)
                    {
                        _logger.Trace("Studio folder didn't exist, creating {0}", filePath);
                        _diskProvider.CreateFolder(filePath);
                    }

                    var files = _diskProvider.GetFiles(filePath, false);
                    if (files.Any())
                    {
                        var info = _diskProvider.GetFileInfo(files.First());
                        extension = info.Extension;
                    }

                    mediaCover.Url = _configFileProvider.UrlBase + @"/MediaCover/studio/" + studioId + "/" + mediaCover.CoverType.ToString().ToLower() + extension;

                    FileInfo file;
                    var fileExists = false;
                    var testPath = Path.Join(filePath, mediaCover.CoverType.ToString() + extension);
                    if (fileInfos != null)
                    {
                        fileExists = fileInfos.TryGetValue(testPath, out file);
                    }
                    else
                    {
                        file = _diskProvider.GetFileInfo(testPath);
                        fileExists = file.Exists;
                    }

                    if (fileExists)
                    {
                        var lastWrite = file.LastWriteTimeUtc;
                        _logger.Trace("Studio cover already exists, last write time {0}", lastWrite);
                        mediaCover.Url += "?lastWrite=" + lastWrite.Ticks;
                    }
                }
            }
        }

        public void ConvertToLocalUrls(IEnumerable<Tuple<int, IEnumerable<MediaCover>>> items, Dictionary<string, FileInfo> coverFileInfos)
        {
            foreach (var movie in items)
            {
                ConvertToLocalUrls(movie.Item1, movie.Item2, coverFileInfos);
            }
        }

        public void ConvertToLocalPerformerUrls(IEnumerable<Tuple<int, IEnumerable<MediaCover>>> items, Dictionary<string, FileInfo> coverFileInfos)
        {
            foreach (var movie in items)
            {
                ConvertToLocalPerformerUrls(movie.Item1, movie.Item2, coverFileInfos);
            }
        }

        public void ConvertToLocalStudioUrls(IEnumerable<Tuple<int, IEnumerable<MediaCover>>> items, Dictionary<string, FileInfo> coverFileInfos)
        {
            foreach (var movie in items)
            {
                ConvertToLocalStudioUrls(movie.Item1, movie.Item2, coverFileInfos);
            }
        }

        private string GetMovieCoverPath(int movieId)
        {
            return Path.Combine(_coverRootFolder, "movie", movieId.ToString());
        }

        private string GetPerformerCoverPath(int performerId)
        {
            return Path.Combine(_coverRootFolder, "performer", performerId.ToString());
        }

        private string GetStudioCoverPath(int studioId)
        {
            return Path.Combine(_coverRootFolder, "studio", studioId.ToString());
        }

        private bool EnsureCovers(Movie movie)
        {
            var updated = false;
            var toResize = new List<Tuple<MediaCover, bool>>();

            foreach (var cover in movie.MovieMetadata.Value.Images)
            {
                if (cover.CoverType == MediaCoverTypes.Unknown)
                {
                    continue;
                }

                var fileName = GetMovieCoverPath(movie.Id, cover.CoverType);
                var alreadyExists = false;

                try
                {
                    alreadyExists = _coverExistsSpecification.AlreadyExists(cover.RemoteUrl, fileName);

                    if (!alreadyExists)
                    {
                        DownloadCover(movie, cover);
                        updated = true;
                    }
                }
                catch (HttpException e)
                {
                    _logger.Warn("Couldn't download media cover for {0}. {1}", movie, e.Message);
                }
                catch (WebException e)
                {
                    _logger.Warn("Couldn't download media cover for {0}. {1}", movie, e.Message);
                }
                catch (Exception e)
                {
                    _logger.Error(e, "Couldn't download media cover for {0}", movie);
                }

                toResize.Add(Tuple.Create(cover, alreadyExists));
            }

            try
            {
                _semaphore.Wait();

                foreach (var tuple in toResize)
                {
                    EnsureResizedCovers(movie, tuple.Item1, !tuple.Item2);
                }
            }
            finally
            {
                _semaphore.Release();
            }

            return updated;
        }

        private bool EnsureCovers(Studio studio)
        {
            var updated = false;
            var toResize = new List<Tuple<MediaCover, bool>>();

            foreach (var cover in studio.Images)
            {
                if (cover.CoverType == MediaCoverTypes.Unknown)
                {
                    continue;
                }

                var fileName = GetStudioCoverPath(studio.Id, cover.CoverType);
                var alreadyExists = false;

                try
                {
                    alreadyExists = _coverExistsSpecification.AlreadyExists(cover.RemoteUrl, fileName);

                    if (!alreadyExists)
                    {
                        DownloadCover(studio, cover);
                        updated = true;
                    }
                }
                catch (HttpException e)
                {
                    _logger.Warn("Couldn't download media cover for {0}. {1}", studio, e.Message);
                }
                catch (WebException e)
                {
                    _logger.Warn("Couldn't download media cover for {0}. {1}", studio, e.Message);
                }
                catch (Exception e)
                {
                    _logger.Error(e, "Couldn't download media cover for {0}", studio);
                }

                toResize.Add(Tuple.Create(cover, alreadyExists));
            }

            try
            {
                _semaphore.Wait();

                foreach (var tuple in toResize)
                {
                    EnsureResizedCovers(studio, tuple.Item1, !tuple.Item2);
                }
            }
            finally
            {
                _semaphore.Release();
            }

            return updated;
        }

        private bool EnsureCovers(Performer performer)
        {
            var updated = false;
            var toResize = new List<Tuple<MediaCover, bool>>();

            foreach (var cover in performer.Images)
            {
                if (cover.CoverType == MediaCoverTypes.Unknown)
                {
                    continue;
                }

                var fileName = GetPerformerCoverPath(performer.Id, cover.CoverType);
                var alreadyExists = false;

                try
                {
                    alreadyExists = _coverExistsSpecification.AlreadyExists(cover.RemoteUrl, fileName);

                    if (!alreadyExists)
                    {
                        DownloadCover(performer, cover);
                        updated = true;
                    }
                }
                catch (HttpException e)
                {
                    _logger.Warn("Couldn't download media cover for {0}. {1}", performer, e.Message);
                }
                catch (WebException e)
                {
                    _logger.Warn("Couldn't download media cover for {0}. {1}", performer, e.Message);
                }
                catch (Exception e)
                {
                    _logger.Error(e, "Couldn't download media cover for {0}", performer);
                }

                toResize.Add(Tuple.Create(cover, alreadyExists));
            }

            try
            {
                _semaphore.Wait();

                foreach (var tuple in toResize)
                {
                    EnsureResizedCovers(performer, tuple.Item1, !tuple.Item2);
                }
            }
            finally
            {
                _semaphore.Release();
            }

            return updated;
        }

        private void DownloadCover(Movie movie, MediaCover cover)
        {
            var fileName = GetMovieCoverPath(movie.Id, cover.CoverType);

            _logger.Info("Downloading {0} for {1} {2}", cover.CoverType, movie, cover.RemoteUrl);
            _httpClient.DownloadFile(cover.RemoteUrl, fileName);
        }

        private void DownloadCover(Performer performer, MediaCover cover)
        {
            var fileName = GetPerformerCoverPath(performer.Id, cover.CoverType);

            _logger.Info("Downloading {0} for {1} {2}", cover.CoverType, performer, cover.RemoteUrl);
            _httpClient.DownloadFile(cover.RemoteUrl, fileName);
        }

        private void DownloadCover(Studio studio, MediaCover cover)
        {
            var req = new HttpRequest(cover.RemoteUrl);
            var imageResponse = _httpClient.Execute(req);
            var extension = imageResponse.Headers.ContentType switch
            {
                "image/svg+xml" => ".svg",
                "image/png" => ".png",
                "image/jpeg " => ".jpg",
                _ => ".png",
            };
            var filePath = GetStudioCoverPath(studio.Id);
            filePath = Path.Join(filePath, cover.CoverType.ToString().ToLower() + extension);
            var fileInfo = _diskProvider.GetFileInfo(filePath);
            if (fileInfo.Directory != null && !fileInfo.Directory.Exists)
            {
                fileInfo.Directory.Create();
            }

            _logger.Trace("Writing studio cover to {0}", filePath);
            File.WriteAllBytes(filePath, imageResponse.ResponseData);
        }

        private bool RemoteUrlHasExtension(MediaCover cover)
        {
            var url = cover.RemoteUrl.Split('?')[0]; // ignore URL params
            url = url.Split('/').Last();
            var extension = url.Contains('.') ? url.Substring(url.LastIndexOf('.')) : "";
            if (extension.Length > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void EnsureResizedCovers(Movie movie, MediaCover cover, bool forceResize)
        {
            int[] heights;

            switch (cover.CoverType)
            {
                default:
                    return;

                case MediaCoverTypes.Poster:
                case MediaCoverTypes.Headshot:
                    heights = new[] { 500, 250 };
                    break;

                case MediaCoverTypes.Banner:
                    heights = new[] { 70, 35 };
                    break;

                case MediaCoverTypes.Fanart:
                case MediaCoverTypes.Screenshot:
                    heights = new[] { 360, 180 };
                    break;
            }

            foreach (var height in heights)
            {
                var mainFileName = GetMovieCoverPath(movie.Id, cover.CoverType);
                var resizeFileName = GetMovieCoverPath(movie.Id, cover.CoverType, height);

                if (forceResize || !_diskProvider.FileExists(resizeFileName) || _diskProvider.GetFileSize(resizeFileName) == 0)
                {
                    _logger.Debug("Resizing {0}-{1} for {2}", cover.CoverType, height, movie);

                    try
                    {
                        _resizer.Resize(mainFileName, resizeFileName, height);
                    }
                    catch
                    {
                        _logger.Debug("Couldn't resize media cover {0}-{1} for {2}, using full size image instead.", cover.CoverType, height, movie);
                    }
                }
            }
        }

        private void EnsureResizedCovers(Studio studio, MediaCover cover, bool forceResize)
        {
            int[] heights;

            switch (cover.CoverType)
            {
                default:
                    return;

                case MediaCoverTypes.Poster:
                case MediaCoverTypes.Headshot:
                    heights = new[] { 500, 250 };
                    break;

                case MediaCoverTypes.Banner:
                    heights = new[] { 70, 35 };
                    break;

                case MediaCoverTypes.Fanart:
                case MediaCoverTypes.Screenshot:
                    heights = new[] { 360, 180 };
                    break;
            }

            foreach (var height in heights)
            {
                var mainFileName = GetStudioCoverPath(studio.Id, cover.CoverType);
                var resizeFileName = GetStudioCoverPath(studio.Id, cover.CoverType, height);

                if (forceResize || !_diskProvider.FileExists(resizeFileName) || _diskProvider.GetFileSize(resizeFileName) == 0)
                {
                    _logger.Debug("Resizing {0}-{1} for {2}", cover.CoverType, height, studio);

                    try
                    {
                        _resizer.Resize(mainFileName, resizeFileName, height);
                    }
                    catch
                    {
                        _logger.Debug("Couldn't resize media cover {0}-{1} for {2}, using full size image instead.", cover.CoverType, height, studio);
                    }
                }
            }
        }

        private void EnsureResizedCovers(Performer performer, MediaCover cover, bool forceResize)
        {
            int[] heights;

            switch (cover.CoverType)
            {
                default:
                    return;

                case MediaCoverTypes.Poster:
                case MediaCoverTypes.Headshot:
                    heights = new[] { 500, 250 };
                    break;

                case MediaCoverTypes.Banner:
                    heights = new[] { 70, 35 };
                    break;

                case MediaCoverTypes.Fanart:
                case MediaCoverTypes.Screenshot:
                    heights = new[] { 360, 180 };
                    break;
            }

            foreach (var height in heights)
            {
                var mainFileName = GetPerformerCoverPath(performer.Id, cover.CoverType);
                var resizeFileName = GetPerformerCoverPath(performer.Id, cover.CoverType, height);

                if (forceResize || !_diskProvider.FileExists(resizeFileName) || _diskProvider.GetFileSize(resizeFileName) == 0)
                {
                    _logger.Debug("Resizing {0}-{1} for {2}", cover.CoverType, height, performer);

                    try
                    {
                        _resizer.Resize(mainFileName, resizeFileName, height);
                    }
                    catch
                    {
                        _logger.Debug("Couldn't resize media cover {0}-{1} for {2}, using full size image instead.", cover.CoverType, height, performer);
                    }
                }
            }
        }

        private string GetExtension(MediaCoverTypes coverType)
        {
            return coverType switch
            {
                MediaCoverTypes.Clearlogo => ".png",
                _ => ".jpg"
            };
        }

        private Dictionary<string, FileInfo> GetCoverFileInfos(string subFolder)
        {
            if (!_diskProvider.FolderExists(Path.Combine(_coverRootFolder, subFolder)))
            {
                return new Dictionary<string, FileInfo>();
            }

            return _diskProvider
                    .GetFileInfos(Path.Combine(_coverRootFolder, subFolder), true)
                    .ToDictionary(x => x.FullName, PathEqualityComparer.Instance);
        }

        public void HandleAsync(MovieUpdatedEvent message)
        {
            var updated = EnsureCovers(message.Movie);

            _eventAggregator.PublishEvent(new MediaCoversUpdatedEvent(message.Movie, updated));
        }

        public void HandleAsync(PerformerUpdatedEvent message)
        {
            var updated = EnsureCovers(message.Performer);
        }

        public void HandleAsync(StudioUpdatedEvent message)
        {
            var updated = EnsureCovers(message.Studio);
        }

        public void HandleAsync(MoviesDeletedEvent message)
        {
            foreach (var movie in message.Movies)
            {
                var path = GetMovieCoverPath(movie.Id);
                if (_diskProvider.FolderExists(path))
                {
                    _diskProvider.DeleteFolder(path, true);
                }
            }
        }
    }
}
