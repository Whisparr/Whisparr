using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Profiles;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.Movies
{
    public interface IMovieCutoffService
    {
        PagingSpec<Media> MoviesWhereCutoffUnmet(PagingSpec<Media> pagingSpec);
    }

    public class MovieCutoffService : IMovieCutoffService
    {
        private readonly IMediaRepository _movieRepository;
        private readonly IProfileService _profileService;

        public MovieCutoffService(IMediaRepository movieRepository, IProfileService profileService, Logger logger)
        {
            _movieRepository = movieRepository;
            _profileService = profileService;
        }

        public PagingSpec<Media> MoviesWhereCutoffUnmet(PagingSpec<Media> pagingSpec)
        {
            var qualitiesBelowCutoff = new List<QualitiesBelowCutoff>();
            var profiles = _profileService.All();

            //Get all items less than the cutoff
            foreach (var profile in profiles)
            {
                var cutoff = profile.UpgradeAllowed ? profile.Cutoff : profile.FirststAllowedQuality().Id;
                var cutoffIndex = profile.GetIndex(cutoff);
                var belowCutoff = profile.Items.Take(cutoffIndex.Index).ToList();

                if (belowCutoff.Any())
                {
                    qualitiesBelowCutoff.Add(new QualitiesBelowCutoff(profile.Id, belowCutoff.SelectMany(i => i.GetQualities().Select(q => q.Id))));
                }
            }

            return _movieRepository.MoviesWhereCutoffUnmet(pagingSpec, qualitiesBelowCutoff);
        }
    }
}
