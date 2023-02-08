using System.Collections.Generic;
using FluentValidation.Results;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Notifications.Stash
{
    public class Stash : NotificationBase<StashSettings>
    {
        private readonly IStashService _stashService;

        public Stash(IStashService stashService)
        {
            _stashService = stashService;
        }

        public override string Link => "https://stashapp.cc/";
        public override string Name => "Stash";

        public override void OnDownload(DownloadMessage message)
        {
            _stashService.Update(Settings, message.Series);
        }

        public override void OnRename(Series series, List<RenamedEpisodeFile> renamedFiles)
        {
            _stashService.Update(Settings, series);
        }

        public override void OnEpisodeFileDelete(EpisodeDeleteMessage deleteMessage)
        {
            _stashService.Update(Settings, deleteMessage.Series);
        }

        public override void OnSeriesDelete(SeriesDeleteMessage deleteMessage)
        {
            _stashService.Update(Settings, deleteMessage.Series);
        }

        public override ValidationResult Test()
        {
            var failures = new List<ValidationFailure>();

            failures.AddIfNotNull(_stashService.Test(Settings));

            return new ValidationResult(failures);
        }
    }
}
