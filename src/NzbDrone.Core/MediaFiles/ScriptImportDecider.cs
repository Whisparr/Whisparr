using System.Collections.Specialized;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Processes;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.MediaFiles.MediaInfo;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Tags;

namespace NzbDrone.Core.MediaFiles
{
    public interface IImportScript
    {
        public ScriptImportDecision TryImport(string sourcePath, string destinationFilePath, LocalEpisode localEpisode, EpisodeFile episodeFile, TransferMode mode);
    }

    public class ImportScriptService : IImportScript
    {
        private readonly IConfigFileProvider _configFileProvider;
        private readonly IVideoFileInfoReader _videoFileInfoReader;
        private readonly IProcessProvider _processProvider;
        private readonly IConfigService _configService;
        private readonly ITagService _tagService;
        private readonly Logger _logger;

        public ImportScriptService(IProcessProvider processProvider,
                                   IVideoFileInfoReader videoFileInfoReader,
                                   IConfigService configService,
                                   IConfigFileProvider configFileProvider,
                                   ITagService tagService,
                                   Logger logger)
        {
            _processProvider = processProvider;
            _videoFileInfoReader = videoFileInfoReader;
            _configService = configService;
            _configFileProvider = configFileProvider;
            _tagService = tagService;
            _logger = logger;
        }

        public ScriptImportDecision TryImport(string sourcePath, string destinationFilePath, LocalEpisode localEpisode, EpisodeFile episodeFile, TransferMode mode)
        {
            var series = localEpisode.Series;
            var oldFiles = localEpisode.OldFiles;
            var downloadClientInfo = localEpisode.DownloadItem?.DownloadClientInfo;
            var downloadId = localEpisode.DownloadItem?.DownloadId;

            if (!_configService.UseScriptImport)
            {
                return ScriptImportDecision.DeferMove;
            }

            var environmentVariables = new StringDictionary();

            environmentVariables.Add("Whisparr_SourcePath", sourcePath);
            environmentVariables.Add("Whisparr_DestinationPath", destinationFilePath);

            environmentVariables.Add("Whisparr_InstanceName", _configFileProvider.InstanceName);
            environmentVariables.Add("Whisparr_ApplicationUrl", _configService.ApplicationUrl);
            environmentVariables.Add("Whisparr_TransferMode", mode.ToString());

            environmentVariables.Add("Whisparr_Series_Id", series.Id.ToString());
            environmentVariables.Add("Whisparr_Series_Title", series.Title);
            environmentVariables.Add("Whisparr_Series_TitleSlug", series.TitleSlug);
            environmentVariables.Add("Whisparr_Series_Path", series.Path);
            environmentVariables.Add("Whisparr_Series_TvdbId", series.TvdbId.ToString());
            environmentVariables.Add("Whisparr_Series_Genres", string.Join("|", series.Genres));
            environmentVariables.Add("Whisparr_Series_Tags", string.Join("|", series.Tags.Select(t => _tagService.GetTag(t).Label)));

            environmentVariables.Add("Whisparr_EpisodeFile_EpisodeCount", localEpisode.Episodes.Count.ToString());
            environmentVariables.Add("Whisparr_EpisodeFile_EpisodeIds", string.Join(",", localEpisode.Episodes.Select(e => e.Id)));
            environmentVariables.Add("Whisparr_EpisodeFile_SeasonNumber", localEpisode.SeasonNumber.ToString());
            environmentVariables.Add("Whisparr_EpisodeFile_EpisodeAirDates", string.Join(",", localEpisode.Episodes.Select(e => e.AirDate)));
            environmentVariables.Add("Whisparr_EpisodeFile_EpisodeAirDatesUtc", string.Join(",", localEpisode.Episodes.Select(e => e.AirDateUtc)));
            environmentVariables.Add("Whisparr_EpisodeFile_EpisodeTitles", string.Join("|", localEpisode.Episodes.Select(e => e.Title)));
            environmentVariables.Add("Whisparr_EpisodeFile_EpisodeOverviews", string.Join("|", localEpisode.Episodes.Select(e => e.Overview)));
            environmentVariables.Add("Whisparr_EpisodeFile_Quality", localEpisode.Quality.Quality.Name);
            environmentVariables.Add("Whisparr_EpisodeFile_QualityVersion", localEpisode.Quality.Revision.Version.ToString());
            environmentVariables.Add("Whisparr_EpisodeFile_ReleaseGroup", localEpisode.ReleaseGroup ?? string.Empty);
            environmentVariables.Add("Whisparr_EpisodeFile_SceneName", localEpisode.SceneName ?? string.Empty);

            environmentVariables.Add("Whisparr_Download_Client", downloadClientInfo?.Name ?? string.Empty);
            environmentVariables.Add("Whisparr_Download_Client_Type", downloadClientInfo?.Type ?? string.Empty);
            environmentVariables.Add("Whisparr_Download_Id", downloadId ?? string.Empty);
            environmentVariables.Add("Whisparr_EpisodeFile_MediaInfo_AudioChannels", MediaInfoFormatter.FormatAudioChannels(localEpisode.MediaInfo).ToString());
            environmentVariables.Add("Whisparr_EpisodeFile_MediaInfo_AudioCodec", MediaInfoFormatter.FormatAudioCodec(localEpisode.MediaInfo, null));
            environmentVariables.Add("Whisparr_EpisodeFile_MediaInfo_AudioLanguages", localEpisode.MediaInfo.AudioLanguages.Distinct().ConcatToString(" / "));
            environmentVariables.Add("Whisparr_EpisodeFile_MediaInfo_Languages", localEpisode.MediaInfo.AudioLanguages.ConcatToString(" / "));
            environmentVariables.Add("Whisparr_EpisodeFile_MediaInfo_Height", localEpisode.MediaInfo.Height.ToString());
            environmentVariables.Add("Whisparr_EpisodeFile_MediaInfo_Width", localEpisode.MediaInfo.Width.ToString());
            environmentVariables.Add("Whisparr_EpisodeFile_MediaInfo_Subtitles", localEpisode.MediaInfo.Subtitles.ConcatToString(" / "));
            environmentVariables.Add("Whisparr_EpisodeFile_MediaInfo_VideoCodec", MediaInfoFormatter.FormatVideoCodec(localEpisode.MediaInfo, null));
            environmentVariables.Add("Whisparr_EpisodeFile_MediaInfo_VideoDynamicRangeType", MediaInfoFormatter.FormatVideoDynamicRangeType(localEpisode.MediaInfo));

            environmentVariables.Add("Whisparr_EpisodeFile_CustomFormat", string.Join("|", localEpisode.CustomFormats));
            environmentVariables.Add("Whisparr_EpisodeFile_CustomFormatScore", localEpisode.CustomFormatScore.ToString());

            if (oldFiles.Any())
            {
                environmentVariables.Add("Whisparr_DeletedRelativePaths", string.Join("|", oldFiles.Select(e => e.RelativePath)));
                environmentVariables.Add("Whisparr_DeletedPaths", string.Join("|", oldFiles.Select(e => Path.Combine(series.Path, e.RelativePath))));
                environmentVariables.Add("Whisparr_DeletedDateAdded", string.Join("|", oldFiles.Select(e => e.DateAdded)));
            }

            _logger.Debug("Executing external script: {0}", _configService.ScriptImportPath);

            var processOutput = _processProvider.StartAndCapture(_configService.ScriptImportPath, $"\"{sourcePath}\" \"{destinationFilePath}\"", environmentVariables);

            _logger.Debug("Executed external script: {0} - Status: {1}", _configService.ScriptImportPath, processOutput.ExitCode);
            _logger.Debug("Script Output: \r\n{0}", string.Join("\r\n", processOutput.Lines));

            switch (processOutput.ExitCode)
            {
                case 0: // Copy complete
                    return ScriptImportDecision.MoveComplete;
                case 2: // Copy complete, file potentially changed, should try renaming again
                    episodeFile.MediaInfo = _videoFileInfoReader.GetMediaInfo(destinationFilePath);
                    episodeFile.Path = null;
                    return ScriptImportDecision.RenameRequested;
                case 3: // Let Whisparr handle it
                    return ScriptImportDecision.DeferMove;
                default: // Error, fail to import
                    throw new ScriptImportException("Moving with script failed! Exit code {0}", processOutput.ExitCode);
            }
        }
    }
}
