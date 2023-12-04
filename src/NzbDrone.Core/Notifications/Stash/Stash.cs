using System.Collections.Generic;
using FluentValidation.Results;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Movies;

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
            _stashService.Update(Settings, message.Movie);
        }

        public override void OnMovieRename(Movie movie, List<RenamedMovieFile> renamedFiles)
        {
            _stashService.Update(Settings, movie);
        }

        public override void OnMovieDelete(MovieDeleteMessage deleteMessage)
        {
            _stashService.Update(Settings, deleteMessage.Movie);
        }

        public override ValidationResult Test()
        {
            var failures = new List<ValidationFailure>();

            failures.AddIfNotNull(_stashService.Test(Settings));

            return new ValidationResult(failures);
        }
    }
}
