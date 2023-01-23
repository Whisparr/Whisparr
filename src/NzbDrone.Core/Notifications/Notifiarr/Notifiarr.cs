using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using FluentValidation.Results;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.HealthCheck;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.MediaInfo;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Notifications.Notifiarr
{
    public class Notifiarr : NotificationBase<NotifiarrSettings>
    {
        private readonly INotifiarrProxy _proxy;

        public Notifiarr(INotifiarrProxy proxy)
        {
            _proxy = proxy;
        }

        public override string Link => "https://notifiarr.com";
        public override string Name => "Notifiarr";

        public override void OnGrab(GrabMessage message)
        {
            var series = message.Series;
            var remoteEpisode = message.Episode;
            var releaseGroup = remoteEpisode.ParsedEpisodeInfo.ReleaseGroup;
            var variables = new StringDictionary();

            variables.Add("Whisparr_EventType", "Grab");
            variables.Add("Whisparr_Series_Id", series.Id.ToString());
            variables.Add("Whisparr_Series_Title", series.Title);
            variables.Add("Whisparr_Series_TvdbId", series.TvdbId.ToString());
            variables.Add("Whisparr_Release_EpisodeCount", remoteEpisode.Episodes.Count.ToString());
            variables.Add("Whisparr_Release_SeasonNumber", remoteEpisode.Episodes.First().SeasonNumber.ToString());
            variables.Add("Whisparr_Release_EpisodeNumbers", string.Join(",", remoteEpisode.Episodes.Select(e => e.EpisodeNumber)));
            variables.Add("Whisparr_Release_AbsoluteEpisodeNumbers", string.Join(",", remoteEpisode.Episodes.Select(e => e.AbsoluteEpisodeNumber)));
            variables.Add("Whisparr_Release_EpisodeAirDates", string.Join(",", remoteEpisode.Episodes.Select(e => e.AirDate)));
            variables.Add("Whisparr_Release_EpisodeAirDatesUtc", string.Join(",", remoteEpisode.Episodes.Select(e => e.AirDateUtc)));
            variables.Add("Whisparr_Release_EpisodeTitles", string.Join("|", remoteEpisode.Episodes.Select(e => e.Title)));
            variables.Add("Whisparr_Release_Title", remoteEpisode.Release.Title);
            variables.Add("Whisparr_Release_Indexer", remoteEpisode.Release.Indexer ?? string.Empty);
            variables.Add("Whisparr_Release_Size", remoteEpisode.Release.Size.ToString());
            variables.Add("Whisparr_Release_Quality", remoteEpisode.ParsedEpisodeInfo.Quality.Quality.Name);
            variables.Add("Whisparr_Release_QualityVersion", remoteEpisode.ParsedEpisodeInfo.Quality.Revision.Version.ToString());
            variables.Add("Whisparr_Release_ReleaseGroup", releaseGroup ?? string.Empty);
            variables.Add("Whisparr_Download_Client", message.DownloadClientName ?? string.Empty);
            variables.Add("Whisparr_Download_Client_Type", message.DownloadClientType ?? string.Empty);
            variables.Add("Whisparr_Download_Id", message.DownloadId ?? string.Empty);
            variables.Add("Whisparr_Release_CustomFormat", string.Join("|", remoteEpisode.CustomFormats));
            variables.Add("Whisparr_Release_CustomFormatScore", remoteEpisode.CustomFormatScore.ToString());

            _proxy.SendNotification(variables, Settings);
        }

        public override void OnDownload(DownloadMessage message)
        {
            var series = message.Series;
            var episodeFile = message.EpisodeFile;
            var sourcePath = message.SourcePath;
            var variables = new StringDictionary();

            variables.Add("Whisparr_EventType", "Download");
            variables.Add("Whisparr_IsUpgrade", message.OldFiles.Any().ToString());
            variables.Add("Whisparr_Series_Id", series.Id.ToString());
            variables.Add("Whisparr_Series_Title", series.Title);
            variables.Add("Whisparr_Series_Path", series.Path);
            variables.Add("Whisparr_Series_TvdbId", series.TvdbId.ToString());
            variables.Add("Whisparr_EpisodeFile_Id", episodeFile.Id.ToString());
            variables.Add("Whisparr_EpisodeFile_EpisodeCount", episodeFile.Episodes.Value.Count.ToString());
            variables.Add("Whisparr_EpisodeFile_RelativePath", episodeFile.RelativePath);
            variables.Add("Whisparr_EpisodeFile_Path", Path.Combine(series.Path, episodeFile.RelativePath));
            variables.Add("Whisparr_EpisodeFile_EpisodeIds", string.Join(",", episodeFile.Episodes.Value.Select(e => e.Id)));
            variables.Add("Whisparr_EpisodeFile_SeasonNumber", episodeFile.SeasonNumber.ToString());
            variables.Add("Whisparr_EpisodeFile_EpisodeNumbers", string.Join(",", episodeFile.Episodes.Value.Select(e => e.EpisodeNumber)));
            variables.Add("Whisparr_EpisodeFile_EpisodeAirDates", string.Join(",", episodeFile.Episodes.Value.Select(e => e.AirDate)));
            variables.Add("Whisparr_EpisodeFile_EpisodeAirDatesUtc", string.Join(",", episodeFile.Episodes.Value.Select(e => e.AirDateUtc)));
            variables.Add("Whisparr_EpisodeFile_EpisodeTitles", string.Join("|", episodeFile.Episodes.Value.Select(e => e.Title)));
            variables.Add("Whisparr_EpisodeFile_Quality", episodeFile.Quality.Quality.Name);
            variables.Add("Whisparr_EpisodeFile_QualityVersion", episodeFile.Quality.Revision.Version.ToString());
            variables.Add("Whisparr_EpisodeFile_ReleaseGroup", episodeFile.ReleaseGroup ?? string.Empty);
            variables.Add("Whisparr_EpisodeFile_SceneName", episodeFile.SceneName ?? string.Empty);
            variables.Add("Whisparr_EpisodeFile_SourcePath", sourcePath);
            variables.Add("Whisparr_EpisodeFile_SourceFolder", Path.GetDirectoryName(sourcePath));
            variables.Add("Whisparr_Download_Client", message.DownloadClientInfo?.Name ?? string.Empty);
            variables.Add("Whisparr_Download_Client_Type", message.DownloadClientInfo?.Type ?? string.Empty);
            variables.Add("Whisparr_Download_Id", message.DownloadId ?? string.Empty);
            variables.Add("Whisparr_EpisodeFile_MediaInfo_AudioChannels", MediaInfoFormatter.FormatAudioChannels(episodeFile.MediaInfo).ToString());
            variables.Add("Whisparr_EpisodeFile_MediaInfo_AudioCodec", MediaInfoFormatter.FormatAudioCodec(episodeFile.MediaInfo, null));
            variables.Add("Whisparr_EpisodeFile_MediaInfo_AudioLanguages", episodeFile.MediaInfo.AudioLanguages.Distinct().ConcatToString(" / "));
            variables.Add("Whisparr_EpisodeFile_MediaInfo_Languages", episodeFile.MediaInfo.AudioLanguages.ConcatToString(" / "));
            variables.Add("Whisparr_EpisodeFile_MediaInfo_Height", episodeFile.MediaInfo.Height.ToString());
            variables.Add("Whisparr_EpisodeFile_MediaInfo_Width", episodeFile.MediaInfo.Width.ToString());
            variables.Add("Whisparr_EpisodeFile_MediaInfo_Subtitles", episodeFile.MediaInfo.Subtitles.ConcatToString(" / "));
            variables.Add("Whisparr_EpisodeFile_MediaInfo_VideoCodec", MediaInfoFormatter.FormatVideoCodec(episodeFile.MediaInfo, null));
            variables.Add("Whisparr_EpisodeFile_MediaInfo_VideoDynamicRangeType", MediaInfoFormatter.FormatVideoDynamicRangeType(episodeFile.MediaInfo));
            variables.Add("Whisparr_EpisodeFile_CustomFormat", string.Join("|", message.EpisodeInfo.CustomFormats));
            variables.Add("Whisparr_EpisodeFile_CustomFormatScore", message.EpisodeInfo.CustomFormatScore.ToString());

            if (message.OldFiles.Any())
            {
                variables.Add("Whisparr_DeletedRelativePaths", string.Join("|", message.OldFiles.Select(e => e.RelativePath)));
                variables.Add("Whisparr_DeletedPaths", string.Join("|", message.OldFiles.Select(e => Path.Combine(series.Path, e.RelativePath))));
                variables.Add("Whisparr_DeletedDateAdded", string.Join("|", message.OldFiles.Select(e => e.DateAdded)));
            }

            _proxy.SendNotification(variables, Settings);
        }

        public override void OnRename(Series series, List<RenamedEpisodeFile> renamedFiles)
        {
            var variables = new StringDictionary();

            variables.Add("Whisparr_EventType", "Rename");
            variables.Add("Whisparr_Series_Id", series.Id.ToString());
            variables.Add("Whisparr_Series_Title", series.Title);
            variables.Add("Whisparr_Series_Path", series.Path);
            variables.Add("Whisparr_Series_TvdbId", series.TvdbId.ToString());
            variables.Add("Whisparr_EpisodeFile_Ids", string.Join(",", renamedFiles.Select(e => e.EpisodeFile.Id)));
            variables.Add("Whisparr_EpisodeFile_RelativePaths", string.Join("|", renamedFiles.Select(e => e.EpisodeFile.RelativePath)));
            variables.Add("Whisparr_EpisodeFile_Paths", string.Join("|", renamedFiles.Select(e => e.EpisodeFile.Path)));
            variables.Add("Whisparr_EpisodeFile_PreviousRelativePaths", string.Join("|", renamedFiles.Select(e => e.PreviousRelativePath)));
            variables.Add("Whisparr_EpisodeFile_PreviousPaths", string.Join("|", renamedFiles.Select(e => e.PreviousPath)));

            _proxy.SendNotification(variables, Settings);
        }

        public override void OnEpisodeFileDelete(EpisodeDeleteMessage deleteMessage)
        {
            var series = deleteMessage.Series;
            var episodeFile = deleteMessage.EpisodeFile;

            var variables = new StringDictionary();

            variables.Add("Whisparr_EventType", "EpisodeFileDelete");
            variables.Add("Whisparr_EpisodeFile_DeleteReason", deleteMessage.Reason.ToString());
            variables.Add("Whisparr_Series_Id", series.Id.ToString());
            variables.Add("Whisparr_Series_Title", series.Title);
            variables.Add("Whisparr_Series_Path", series.Path);
            variables.Add("Whisparr_Series_TvdbId", series.TvdbId.ToString());
            variables.Add("Whisparr_EpisodeFile_Id", episodeFile.Id.ToString());
            variables.Add("Whisparr_EpisodeFile_EpisodeCount", episodeFile.Episodes.Value.Count.ToString());
            variables.Add("Whisparr_EpisodeFile_RelativePath", episodeFile.RelativePath);
            variables.Add("Whisparr_EpisodeFile_Path", Path.Combine(series.Path, episodeFile.RelativePath));
            variables.Add("Whisparr_EpisodeFile_EpisodeIds", string.Join(",", episodeFile.Episodes.Value.Select(e => e.Id)));
            variables.Add("Whisparr_EpisodeFile_SeasonNumber", episodeFile.SeasonNumber.ToString());
            variables.Add("Whisparr_EpisodeFile_EpisodeNumbers", string.Join(",", episodeFile.Episodes.Value.Select(e => e.EpisodeNumber)));
            variables.Add("Whisparr_EpisodeFile_EpisodeAirDates", string.Join(",", episodeFile.Episodes.Value.Select(e => e.AirDate)));
            variables.Add("Whisparr_EpisodeFile_EpisodeAirDatesUtc", string.Join(",", episodeFile.Episodes.Value.Select(e => e.AirDateUtc)));
            variables.Add("Whisparr_EpisodeFile_EpisodeTitles", string.Join("|", episodeFile.Episodes.Value.Select(e => e.Title)));
            variables.Add("Whisparr_EpisodeFile_Quality", episodeFile.Quality.Quality.Name);
            variables.Add("Whisparr_EpisodeFile_QualityVersion", episodeFile.Quality.Revision.Version.ToString());
            variables.Add("Whisparr_EpisodeFile_ReleaseGroup", episodeFile.ReleaseGroup ?? string.Empty);
            variables.Add("Whisparr_EpisodeFile_SceneName", episodeFile.SceneName ?? string.Empty);

            _proxy.SendNotification(variables, Settings);
        }

        public override void OnSeriesDelete(SeriesDeleteMessage deleteMessage)
        {
            var series = deleteMessage.Series;
            var variables = new StringDictionary();

            variables.Add("Whisparr_EventType", "SeriesDelete");
            variables.Add("Whisparr_Series_Id", series.Id.ToString());
            variables.Add("Whisparr_Series_Title", series.Title);
            variables.Add("Whisparr_Series_Path", series.Path);
            variables.Add("Whisparr_Series_TvdbId", series.TvdbId.ToString());
            variables.Add("Whisparr_Series_DeletedFiles", deleteMessage.DeletedFiles.ToString());

            _proxy.SendNotification(variables, Settings);
        }

        public override void OnHealthIssue(HealthCheck.HealthCheck healthCheck)
        {
            var variables = new StringDictionary();

            variables.Add("Whisparr_EventType", "HealthIssue");
            variables.Add("Whisparr_Health_Issue_Level", Enum.GetName(typeof(HealthCheckResult), healthCheck.Type));
            variables.Add("Whisparr_Health_Issue_Message", healthCheck.Message);
            variables.Add("Whisparr_Health_Issue_Type", healthCheck.Source.Name);
            variables.Add("Whisparr_Health_Issue_Wiki", healthCheck.WikiUrl.ToString() ?? string.Empty);

            _proxy.SendNotification(variables, Settings);
        }

        public override void OnApplicationUpdate(ApplicationUpdateMessage updateMessage)
        {
            var variables = new StringDictionary();

            variables.Add("Whisparr_EventType", "ApplicationUpdate");
            variables.Add("Whisparr_Update_Message", updateMessage.Message);
            variables.Add("Whisparr_Update_NewVersion", updateMessage.NewVersion.ToString());
            variables.Add("Whisparr_Update_PreviousVersion", updateMessage.PreviousVersion.ToString());

            _proxy.SendNotification(variables, Settings);
        }

        public override ValidationResult Test()
        {
            var failures = new List<ValidationFailure>();

            failures.AddIfNotNull(_proxy.Test(Settings));

            return new ValidationResult(failures);
        }
    }
}
