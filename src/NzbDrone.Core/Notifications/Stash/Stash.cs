using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation.Results;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Profiles.Qualities;
using NzbDrone.Core.RootFolders;

namespace NzbDrone.Core.Notifications.Stash
{
    public class Stash : NotificationBase<StashSettings>
    {
        private readonly IStashService _stashService;
        private readonly IRootFolderService _rootFolderService;
        private readonly IQualityProfileService _qualityProfileService;

        public Stash(IStashService stashService, IRootFolderService rootFolderService, IQualityProfileService qualityProfileService)
        {
            _stashService = stashService;
            _rootFolderService = rootFolderService;
            _qualityProfileService = qualityProfileService;
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
            _stashService.Clean(Settings, movie);
        }

        public override void OnMovieFileDelete(MovieFileDeleteMessage deleteMessage)
        {
            _stashService.Clean(Settings, deleteMessage.Movie);
        }

        public override void OnMovieDelete(MovieDeleteMessage deleteMessage)
        {
            _stashService.Clean(Settings, deleteMessage.Movie);
        }

        public override ValidationResult Test()
        {
            var failures = new List<ValidationFailure>();

            failures.AddIfNotNull(_stashService.Test(Settings));

            return new ValidationResult(failures);
        }

        public override object RequestAction(string action, IDictionary<string, string> query)
        {
            if (action == "getRootFolders")
            {
                return new
                {
                    options = _rootFolderService.All()
                        .OrderBy(folder => folder.Path, StringComparer.InvariantCultureIgnoreCase)
                        .Select(folder => new
                        {
                            Value = folder.Path,
                            Name = folder.Path
                        })
                };
            }

            if (action == "getQualityProfiles")
            {
                return new
                {
                    options = _qualityProfileService.All()
                        .OrderBy(profile => profile.Name, StringComparer.InvariantCultureIgnoreCase)
                        .Select(profile => new
                        {
                            Value = profile.Id,
                            Name = profile.Name
                        })
                };
            }

            return new { };
        }
    }
}
