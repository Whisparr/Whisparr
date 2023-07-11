using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Processes;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.HealthCheck;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.MediaInfo;
using NzbDrone.Core.Tags;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Notifications.CustomScript
{
    public class CustomScript : NotificationBase<CustomScriptSettings>
    {
        private readonly IConfigFileProvider _configFileProvider;
        private readonly IConfigService _configService;
        private readonly IDiskProvider _diskProvider;
        private readonly IProcessProvider _processProvider;
        private readonly ITagRepository _tagRepository;
        private readonly Logger _logger;

        public CustomScript(IConfigFileProvider configFileProvider,
            IConfigService configService,
            IDiskProvider diskProvider,
            IProcessProvider processProvider,
            ITagRepository tagRepository,
            Logger logger)
        {
            _configFileProvider = configFileProvider;
            _configService = configService;
            _diskProvider = diskProvider;
            _processProvider = processProvider;
            _tagRepository = tagRepository;
            _logger = logger;
        }

        public override string Name => "Custom Script";

        public override string Link => "https://wiki.servarr.com/whisparr/settings#connections";

        public override ProviderMessage Message => new ProviderMessage("Testing will execute the script with the EventType set to Test, ensure your script handles this correctly", ProviderMessageType.Warning);

        public override void OnGrab(GrabMessage message)
        {
            var series = message.Series;
            var remoteEpisode = message.Episode;
            var releaseGroup = remoteEpisode.ParsedEpisodeInfo.ReleaseGroup;
            var environmentVariables = new StringDictionary();

            environmentVariables.Add("Whisparr_EventType", "Grab");
            environmentVariables.Add("Whisparr_InstanceName", _configFileProvider.InstanceName);
            environmentVariables.Add("Whisparr_ApplicationUrl", _configService.ApplicationUrl);
            environmentVariables.Add("Whisparr_Series_Id", series.Id.ToString());
            environmentVariables.Add("Whisparr_Series_Title", series.Title);
            environmentVariables.Add("Whisparr_Series_TitleSlug", series.TitleSlug);
            environmentVariables.Add("Whisparr_Series_TvdbId", series.TvdbId.ToString());
            environmentVariables.Add("Whisparr_Series_Genres", string.Join("|", series.Genres));
            environmentVariables.Add("Whisparr_Series_Tags", string.Join("|", series.Tags.Select(t => _tagRepository.Get(t).Label)));
            environmentVariables.Add("Whisparr_Release_EpisodeCount", remoteEpisode.Episodes.Count.ToString());
            environmentVariables.Add("Whisparr_Release_SeasonNumber", remoteEpisode.Episodes.First().SeasonNumber.ToString());
            environmentVariables.Add("Whisparr_Release_AbsoluteEpisodeNumbers", string.Join(",", remoteEpisode.Episodes.Select(e => e.AbsoluteEpisodeNumber)));
            environmentVariables.Add("Whisparr_Release_EpisodeAirDates", string.Join(",", remoteEpisode.Episodes.Select(e => e.AirDate)));
            environmentVariables.Add("Whisparr_Release_EpisodeTitles", string.Join("|", remoteEpisode.Episodes.Select(e => e.Title)));
            environmentVariables.Add("Whisparr_Release_EpisodeOverviews", string.Join("|", remoteEpisode.Episodes.Select(e => e.Overview)));
            environmentVariables.Add("Whisparr_Release_Title", remoteEpisode.Release.Title);
            environmentVariables.Add("Whisparr_Release_Indexer", remoteEpisode.Release.Indexer ?? string.Empty);
            environmentVariables.Add("Whisparr_Release_Size", remoteEpisode.Release.Size.ToString());
            environmentVariables.Add("Whisparr_Release_Quality", remoteEpisode.ParsedEpisodeInfo.Quality.Quality.Name);
            environmentVariables.Add("Whisparr_Release_QualityVersion", remoteEpisode.ParsedEpisodeInfo.Quality.Revision.Version.ToString());
            environmentVariables.Add("Whisparr_Release_ReleaseGroup", releaseGroup ?? string.Empty);
            environmentVariables.Add("Whisparr_Download_Client", message.DownloadClientName ?? string.Empty);
            environmentVariables.Add("Whisparr_Download_Client_Type", message.DownloadClientType ?? string.Empty);
            environmentVariables.Add("Whisparr_Download_Id", message.DownloadId ?? string.Empty);
            environmentVariables.Add("Whisparr_Release_CustomFormat", string.Join("|", remoteEpisode.CustomFormats));
            environmentVariables.Add("Whisparr_Release_CustomFormatScore", remoteEpisode.CustomFormatScore.ToString());

            ExecuteScript(environmentVariables);
        }

        public override void OnDownload(DownloadMessage message)
        {
            var series = message.Series;
            var episodeFile = message.EpisodeFile;
            var sourcePath = message.SourcePath;
            var environmentVariables = new StringDictionary();

            environmentVariables.Add("Whisparr_EventType", "Download");
            environmentVariables.Add("Whisparr_InstanceName", _configFileProvider.InstanceName);
            environmentVariables.Add("Whisparr_ApplicationUrl", _configService.ApplicationUrl);
            environmentVariables.Add("Whisparr_IsUpgrade", message.OldFiles.Any().ToString());
            environmentVariables.Add("Whisparr_Series_Id", series.Id.ToString());
            environmentVariables.Add("Whisparr_Series_Title", series.Title);
            environmentVariables.Add("Whisparr_Series_TitleSlug", series.TitleSlug);
            environmentVariables.Add("Whisparr_Series_Path", series.Path);
            environmentVariables.Add("Whisparr_Series_TvdbId", series.TvdbId.ToString());
            environmentVariables.Add("Whisparr_Series_Genres", string.Join("|", series.Genres));
            environmentVariables.Add("Whisparr_Series_Tags", string.Join("|", series.Tags.Select(t => _tagRepository.Get(t).Label)));
            environmentVariables.Add("Whisparr_EpisodeFile_Id", episodeFile.Id.ToString());
            environmentVariables.Add("Whisparr_EpisodeFile_EpisodeCount", episodeFile.Episodes.Value.Count.ToString());
            environmentVariables.Add("Whisparr_EpisodeFile_RelativePath", episodeFile.RelativePath);
            environmentVariables.Add("Whisparr_EpisodeFile_Path", Path.Combine(series.Path, episodeFile.RelativePath));
            environmentVariables.Add("Whisparr_EpisodeFile_EpisodeIds", string.Join(",", episodeFile.Episodes.Value.Select(e => e.Id)));
            environmentVariables.Add("Whisparr_EpisodeFile_SeasonNumber", episodeFile.SeasonNumber.ToString());
            environmentVariables.Add("Whisparr_EpisodeFile_EpisodeAirDates", string.Join(",", episodeFile.Episodes.Value.Select(e => e.AirDate)));
            environmentVariables.Add("Whisparr_EpisodeFile_EpisodeTitles", string.Join("|", episodeFile.Episodes.Value.Select(e => e.Title)));
            environmentVariables.Add("Whisparr_EpisodeFile_EpisodeOverviews", string.Join("|", episodeFile.Episodes.Value.Select(e => e.Overview)));
            environmentVariables.Add("Whisparr_EpisodeFile_Quality", episodeFile.Quality.Quality.Name);
            environmentVariables.Add("Whisparr_EpisodeFile_QualityVersion", episodeFile.Quality.Revision.Version.ToString());
            environmentVariables.Add("Whisparr_EpisodeFile_ReleaseGroup", episodeFile.ReleaseGroup ?? string.Empty);
            environmentVariables.Add("Whisparr_EpisodeFile_SceneName", episodeFile.SceneName ?? string.Empty);
            environmentVariables.Add("Whisparr_EpisodeFile_SourcePath", sourcePath);
            environmentVariables.Add("Whisparr_EpisodeFile_SourceFolder", Path.GetDirectoryName(sourcePath));
            environmentVariables.Add("Whisparr_Download_Client", message.DownloadClientInfo?.Name ?? string.Empty);
            environmentVariables.Add("Whisparr_Download_Client_Type", message.DownloadClientInfo?.Type ?? string.Empty);
            environmentVariables.Add("Whisparr_Download_Id", message.DownloadId ?? string.Empty);
            environmentVariables.Add("Whisparr_EpisodeFile_MediaInfo_AudioChannels", MediaInfoFormatter.FormatAudioChannels(episodeFile.MediaInfo).ToString());
            environmentVariables.Add("Whisparr_EpisodeFile_MediaInfo_AudioCodec", MediaInfoFormatter.FormatAudioCodec(episodeFile.MediaInfo, null));
            environmentVariables.Add("Whisparr_EpisodeFile_MediaInfo_AudioLanguages", episodeFile.MediaInfo.AudioLanguages.Distinct().ConcatToString(" / "));
            environmentVariables.Add("Whisparr_EpisodeFile_MediaInfo_Languages", episodeFile.MediaInfo.AudioLanguages.ConcatToString(" / "));
            environmentVariables.Add("Whisparr_EpisodeFile_MediaInfo_Height", episodeFile.MediaInfo.Height.ToString());
            environmentVariables.Add("Whisparr_EpisodeFile_MediaInfo_Width", episodeFile.MediaInfo.Width.ToString());
            environmentVariables.Add("Whisparr_EpisodeFile_MediaInfo_Subtitles", episodeFile.MediaInfo.Subtitles.ConcatToString(" / "));
            environmentVariables.Add("Whisparr_EpisodeFile_MediaInfo_VideoCodec", MediaInfoFormatter.FormatVideoCodec(episodeFile.MediaInfo, null));
            environmentVariables.Add("Whisparr_EpisodeFile_MediaInfo_VideoDynamicRangeType", MediaInfoFormatter.FormatVideoDynamicRangeType(episodeFile.MediaInfo));
            environmentVariables.Add("Whisparr_EpisodeFile_CustomFormat", string.Join("|", message.EpisodeInfo.CustomFormats));
            environmentVariables.Add("Whisparr_EpisodeFile_CustomFormatScore", message.EpisodeInfo.CustomFormatScore.ToString());
            environmentVariables.Add("Whisparr_Release_Indexer", message.Release?.Indexer);
            environmentVariables.Add("Whisparr_Release_Size", message.Release?.Size.ToString());
            environmentVariables.Add("Whisparr_Release_Title", message.Release?.Title);

            if (message.OldFiles.Any())
            {
                environmentVariables.Add("Whisparr_DeletedRelativePaths", string.Join("|", message.OldFiles.Select(e => e.RelativePath)));
                environmentVariables.Add("Whisparr_DeletedPaths", string.Join("|", message.OldFiles.Select(e => Path.Combine(series.Path, e.RelativePath))));
                environmentVariables.Add("Whisparr_DeletedDateAdded", string.Join("|", message.OldFiles.Select(e => e.DateAdded)));
            }

            ExecuteScript(environmentVariables);
        }

        public override void OnRename(Series series, List<RenamedEpisodeFile> renamedFiles)
        {
            var environmentVariables = new StringDictionary();

            environmentVariables.Add("Whisparr_EventType", "Rename");
            environmentVariables.Add("Whisparr_InstanceName", _configFileProvider.InstanceName);
            environmentVariables.Add("Whisparr_ApplicationUrl", _configService.ApplicationUrl);
            environmentVariables.Add("Whisparr_Series_Id", series.Id.ToString());
            environmentVariables.Add("Whisparr_Series_Title", series.Title);
            environmentVariables.Add("Whisparr_Series_TitleSlug", series.TitleSlug);
            environmentVariables.Add("Whisparr_Series_Path", series.Path);
            environmentVariables.Add("Whisparr_Series_TvdbId", series.TvdbId.ToString());
            environmentVariables.Add("Whisparr_Series_Genres", string.Join("|", series.Genres));
            environmentVariables.Add("Whisparr_Series_Tags", string.Join("|", series.Tags.Select(t => _tagRepository.Get(t).Label)));
            environmentVariables.Add("Whisparr_EpisodeFile_Ids", string.Join(",", renamedFiles.Select(e => e.EpisodeFile.Id)));
            environmentVariables.Add("Whisparr_EpisodeFile_RelativePaths", string.Join("|", renamedFiles.Select(e => e.EpisodeFile.RelativePath)));
            environmentVariables.Add("Whisparr_EpisodeFile_Paths", string.Join("|", renamedFiles.Select(e => e.EpisodeFile.Path)));
            environmentVariables.Add("Whisparr_EpisodeFile_PreviousRelativePaths", string.Join("|", renamedFiles.Select(e => e.PreviousRelativePath)));
            environmentVariables.Add("Whisparr_EpisodeFile_PreviousPaths", string.Join("|", renamedFiles.Select(e => e.PreviousPath)));

            ExecuteScript(environmentVariables);
        }

        public override void OnEpisodeFileDelete(EpisodeDeleteMessage deleteMessage)
        {
            var series = deleteMessage.Series;
            var episodeFile = deleteMessage.EpisodeFile;

            var environmentVariables = new StringDictionary();

            environmentVariables.Add("Whisparr_EventType", "EpisodeFileDelete");
            environmentVariables.Add("Whisparr_InstanceName", _configFileProvider.InstanceName);
            environmentVariables.Add("Whisparr_ApplicationUrl", _configService.ApplicationUrl);
            environmentVariables.Add("Whisparr_EpisodeFile_DeleteReason", deleteMessage.Reason.ToString());
            environmentVariables.Add("Whisparr_Series_Id", series.Id.ToString());
            environmentVariables.Add("Whisparr_Series_Title", series.Title);
            environmentVariables.Add("Whisparr_Series_TitleSlug", series.TitleSlug);
            environmentVariables.Add("Whisparr_Series_Path", series.Path);
            environmentVariables.Add("Whisparr_Series_TvdbId", series.TvdbId.ToString());
            environmentVariables.Add("Whisparr_Series_Genres", string.Join("|", series.Genres));
            environmentVariables.Add("Whisparr_Series_Tags", string.Join("|", series.Tags.Select(t => _tagRepository.Get(t).Label)));
            environmentVariables.Add("Whisparr_EpisodeFile_Id", episodeFile.Id.ToString());
            environmentVariables.Add("Whisparr_EpisodeFile_EpisodeCount", episodeFile.Episodes.Value.Count.ToString());
            environmentVariables.Add("Whisparr_EpisodeFile_RelativePath", episodeFile.RelativePath);
            environmentVariables.Add("Whisparr_EpisodeFile_Path", Path.Combine(series.Path, episodeFile.RelativePath));
            environmentVariables.Add("Whisparr_EpisodeFile_EpisodeIds", string.Join(",", episodeFile.Episodes.Value.Select(e => e.Id)));
            environmentVariables.Add("Whisparr_EpisodeFile_SeasonNumber", episodeFile.SeasonNumber.ToString());
            environmentVariables.Add("Whisparr_EpisodeFile_EpisodeAirDates", string.Join(",", episodeFile.Episodes.Value.Select(e => e.AirDate)));
            environmentVariables.Add("Whisparr_EpisodeFile_EpisodeTitles", string.Join("|", episodeFile.Episodes.Value.Select(e => e.Title)));
            environmentVariables.Add("Whisparr_EpisodeFile_EpisodeOverviews", string.Join("|", episodeFile.Episodes.Value.Select(e => e.Overview)));
            environmentVariables.Add("Whisparr_EpisodeFile_Quality", episodeFile.Quality.Quality.Name);
            environmentVariables.Add("Whisparr_EpisodeFile_QualityVersion", episodeFile.Quality.Revision.Version.ToString());
            environmentVariables.Add("Whisparr_EpisodeFile_ReleaseGroup", episodeFile.ReleaseGroup ?? string.Empty);
            environmentVariables.Add("Whisparr_EpisodeFile_SceneName", episodeFile.SceneName ?? string.Empty);

            ExecuteScript(environmentVariables);
        }

        public override void OnSeriesAdd(SeriesAddMessage message)
        {
            var series = message.Series;
            var environmentVariables = new StringDictionary();

            environmentVariables.Add("Whisparr_EventType", "SeriesAdd");
            environmentVariables.Add("Whisparr_InstanceName", _configFileProvider.InstanceName);
            environmentVariables.Add("Whisparr_ApplicationUrl", _configService.ApplicationUrl);
            environmentVariables.Add("Whisparr_Series_Id", series.Id.ToString());
            environmentVariables.Add("Whisparr_Series_Title", series.Title);
            environmentVariables.Add("Whisparr_Series_TitleSlug", series.TitleSlug);
            environmentVariables.Add("Whisparr_Series_Path", series.Path);
            environmentVariables.Add("Whisparr_Series_TvdbId", series.TvdbId.ToString());
            environmentVariables.Add("Whisparr_Series_Year", series.Year.ToString());
            environmentVariables.Add("Whisparr_Series_Genres", string.Join("|", series.Genres));
            environmentVariables.Add("Whisparr_Series_Tags", string.Join("|", series.Tags.Select(t => _tagRepository.Get(t).Label)));

            ExecuteScript(environmentVariables);
        }

        public override void OnSeriesDelete(SeriesDeleteMessage deleteMessage)
        {
            var series = deleteMessage.Series;
            var environmentVariables = new StringDictionary();

            environmentVariables.Add("Whisparr_EventType", "SeriesDelete");
            environmentVariables.Add("Whisparr_InstanceName", _configFileProvider.InstanceName);
            environmentVariables.Add("Whisparr_ApplicationUrl", _configService.ApplicationUrl);
            environmentVariables.Add("Whisparr_Series_Id", series.Id.ToString());
            environmentVariables.Add("Whisparr_Series_Title", series.Title);
            environmentVariables.Add("Whisparr_Series_TitleSlug", series.TitleSlug);
            environmentVariables.Add("Whisparr_Series_Path", series.Path);
            environmentVariables.Add("Whisparr_Series_TvdbId", series.TvdbId.ToString());
            environmentVariables.Add("Whisparr_Series_Genres", string.Join("|", series.Genres));
            environmentVariables.Add("Whisparr_Series_Tags", string.Join("|", series.Tags.Select(t => _tagRepository.Get(t).Label)));
            environmentVariables.Add("Whisparr_Series_DeletedFiles", deleteMessage.DeletedFiles.ToString());

            ExecuteScript(environmentVariables);
        }

        public override void OnHealthIssue(HealthCheck.HealthCheck healthCheck)
        {
            var environmentVariables = new StringDictionary();

            environmentVariables.Add("Whisparr_EventType", "HealthIssue");
            environmentVariables.Add("Whisparr_InstanceName", _configFileProvider.InstanceName);
            environmentVariables.Add("Whisparr_ApplicationUrl", _configService.ApplicationUrl);
            environmentVariables.Add("Whisparr_Health_Issue_Level", Enum.GetName(typeof(HealthCheckResult), healthCheck.Type));
            environmentVariables.Add("Whisparr_Health_Issue_Message", healthCheck.Message);
            environmentVariables.Add("Whisparr_Health_Issue_Type", healthCheck.Source.Name);
            environmentVariables.Add("Whisparr_Health_Issue_Wiki", healthCheck.WikiUrl.ToString() ?? string.Empty);

            ExecuteScript(environmentVariables);
        }

        public override void OnHealthRestored(HealthCheck.HealthCheck previousCheck)
        {
            var environmentVariables = new StringDictionary();

            environmentVariables.Add("Whisparr_EventType", "HealthRestored");
            environmentVariables.Add("Whisparr_InstanceName", _configFileProvider.InstanceName);
            environmentVariables.Add("Whisparr_ApplicationUrl", _configService.ApplicationUrl);
            environmentVariables.Add("Whisparr_Health_Restored_Level", Enum.GetName(typeof(HealthCheckResult), previousCheck.Type));
            environmentVariables.Add("Whisparr_Health_Restored_Message", previousCheck.Message);
            environmentVariables.Add("Whisparr_Health_Restored_Type", previousCheck.Source.Name);
            environmentVariables.Add("Whisparr_Health_Restored_Wiki", previousCheck.WikiUrl.ToString() ?? string.Empty);

            ExecuteScript(environmentVariables);
        }

        public override void OnApplicationUpdate(ApplicationUpdateMessage updateMessage)
        {
            var environmentVariables = new StringDictionary();

            environmentVariables.Add("Whisparr_EventType", "ApplicationUpdate");
            environmentVariables.Add("Whisparr_InstanceName", _configFileProvider.InstanceName);
            environmentVariables.Add("Whisparr_ApplicationUrl", _configService.ApplicationUrl);
            environmentVariables.Add("Whisparr_Update_Message", updateMessage.Message);
            environmentVariables.Add("Whisparr_Update_NewVersion", updateMessage.NewVersion.ToString());
            environmentVariables.Add("Whisparr_Update_PreviousVersion", updateMessage.PreviousVersion.ToString());

            ExecuteScript(environmentVariables);
        }

        public override void OnManualInteractionRequired(ManualInteractionRequiredMessage message)
        {
            var series = message.Series;
            var environmentVariables = new StringDictionary();

            environmentVariables.Add("Whisparr_EventType", "ManualInteractionRequired");
            environmentVariables.Add("Whisparr_InstanceName", _configFileProvider.InstanceName);
            environmentVariables.Add("Whisparr_ApplicationUrl", _configService.ApplicationUrl);
            environmentVariables.Add("Whisparr_Series_Id", series.Id.ToString());
            environmentVariables.Add("Whisparr_Series_Title", series.Title);
            environmentVariables.Add("Whisparr_Series_TitleSlug", series.TitleSlug);
            environmentVariables.Add("Whisparr_Series_Path", series.Path);
            environmentVariables.Add("Whisparr_Series_TvdbId", series.TvdbId.ToString());
            environmentVariables.Add("Whisparr_Series_Year", series.Year.ToString());
            environmentVariables.Add("Whisparr_Series_Genres", string.Join("|", series.Genres));
            environmentVariables.Add("Whisparr_Series_Tags", string.Join("|", series.Tags.Select(t => _tagRepository.Get(t).Label)));
            environmentVariables.Add("Whisparr_Download_Client", message.DownloadClientName ?? string.Empty);
            environmentVariables.Add("Whisparr_Download_Client_Type", message.DownloadClientType ?? string.Empty);
            environmentVariables.Add("Whisparr_Download_Id", message.DownloadId ?? string.Empty);
            environmentVariables.Add("Whisparr_Download_Size", message.TrackedDownload.DownloadItem.TotalSize.ToString());
            environmentVariables.Add("Whisparr_Download_Title", message.TrackedDownload.DownloadItem.Title);

            ExecuteScript(environmentVariables);
        }

        public override ValidationResult Test()
        {
            var failures = new List<ValidationFailure>();

            if (!_diskProvider.FileExists(Settings.Path))
            {
                failures.Add(new NzbDroneValidationFailure("Path", "File does not exist"));
            }

            foreach (var systemFolder in SystemFolders.GetSystemFolders())
            {
                if (systemFolder.IsParentPath(Settings.Path))
                {
                    failures.Add(new NzbDroneValidationFailure("Path", $"Must not be a descendant of '{systemFolder}'"));
                }
            }

            if (failures.Empty())
            {
                try
                {
                    var environmentVariables = new StringDictionary();
                    environmentVariables.Add("Whisparr_EventType", "Test");
                    environmentVariables.Add("Whisparr_InstanceName", _configFileProvider.InstanceName);
                    environmentVariables.Add("Whisparr_ApplicationUrl", _configService.ApplicationUrl);

                    var processOutput = ExecuteScript(environmentVariables);

                    if (processOutput.ExitCode != 0)
                    {
                        failures.Add(new NzbDroneValidationFailure(string.Empty, $"Script exited with code: {processOutput.ExitCode}"));
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(ex);
                    failures.Add(new NzbDroneValidationFailure(string.Empty, ex.Message));
                }
            }

            return new ValidationResult(failures);
        }

        private ProcessOutput ExecuteScript(StringDictionary environmentVariables)
        {
            _logger.Debug("Executing external script: {0}", Settings.Path);

            var processOutput = _processProvider.StartAndCapture(Settings.Path, Settings.Arguments, environmentVariables);

            _logger.Debug("Executed external script: {0} - Status: {1}", Settings.Path, processOutput.ExitCode);
            _logger.Debug("Script Output: \r\n{0}", string.Join("\r\n", processOutput.Lines));

            return processOutput;
        }

        private bool ValidatePathParent(string possibleParent, string path)
        {
            return possibleParent.IsParentPath(path);
        }
    }
}
