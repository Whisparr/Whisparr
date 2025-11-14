using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.Datastore.Events;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.Download;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Movies;
using NzbDrone.Core.MovieStats;
using NzbDrone.SignalR;
using Whisparr.Http.REST;

namespace Whisparr.Api.V3.Movies
{
    public abstract class MovieControllerWithSignalR : RestControllerWithSignalR<MovieResource, Movie>,
                                                         IHandle<MovieGrabbedEvent>,
                                                         IHandle<MovieFileImportedEvent>,
                                                         IHandle<MovieFileDeletedEvent>
    {
        protected readonly IMovieService _movieService;
        protected readonly IMovieStatisticsService _movieStatisticsService;
        protected readonly IUpgradableSpecification _upgradableSpecification;
        protected readonly ICustomFormatCalculationService _formatCalculator;
        protected readonly IConfigService _configService;

        protected MovieControllerWithSignalR(IMovieService movieService,
                                           IMovieStatisticsService movieStatisticsService,
                                           IUpgradableSpecification upgradableSpecification,
                                           ICustomFormatCalculationService formatCalculator,
                                           IConfigService configService,
                                           IBroadcastSignalRMessage signalRBroadcaster)
            : base(signalRBroadcaster)
        {
            _movieService = movieService;
            _movieStatisticsService = movieStatisticsService;
            _upgradableSpecification = upgradableSpecification;
            _formatCalculator = formatCalculator;
            _configService = configService;
        }

        protected MovieControllerWithSignalR(IMovieService movieService,
                                           IUpgradableSpecification upgradableSpecification,
                                           ICustomFormatCalculationService formatCalculator,
                                           IBroadcastSignalRMessage signalRBroadcaster,
                                           string resource)
            : base(signalRBroadcaster)
        {
            _movieService = movieService;
            _upgradableSpecification = upgradableSpecification;
            _formatCalculator = formatCalculator;
        }

        protected override MovieResource GetResourceById(int id)
        {
            var movie = _movieService.GetMovie(id);
            var resource = MapToResource(movie);
            return resource;
        }

        protected MovieResource MapToResource(Movie movie)
        {
            if (movie == null)
            {
                return null;
            }

            var availDelay = _configService.AvailabilityDelay;

            var resource = movie.ToResource(availDelay, _upgradableSpecification, _formatCalculator);
            FetchAndLinkMovieStatistics(resource);

            return resource;
        }

        protected List<MovieResource> MapToResource(List<Movie> movies)
        {
            var resources = new List<MovieResource>();
            var availDelay = _configService.AvailabilityDelay;

            foreach (var movie in movies)
            {
                if (movie == null)
                {
                    continue;
                }

                var resource = movie.ToResource(availDelay, _upgradableSpecification, _formatCalculator);
                FetchAndLinkMovieStatistics(resource);

                resources.Add(resource);
            }

            return resources;
        }

        private void FetchAndLinkMovieStatistics(MovieResource resource)
        {
            LinkMovieStatistics(resource, _movieStatisticsService.MovieStatistics(resource.Id));
        }

        private void LinkMovieStatistics(MovieResource resource, MovieStatistics movieStatistics)
        {
            resource.Statistics = movieStatistics.ToResource();
            resource.HasFile = movieStatistics.MovieFileCount > 0;
            resource.SizeOnDisk = movieStatistics.SizeOnDisk;
        }

        [NonAction]
        public void Handle(MovieGrabbedEvent message)
        {
            var resource = message.Movie.Movie.ToResource(0, _upgradableSpecification, _formatCalculator);
            resource.Grabbed = true;

            BroadcastResourceChange(ModelAction.Updated, resource);
        }

        [NonAction]
        public void Handle(MovieFileImportedEvent message)
        {
            BroadcastResourceChange(ModelAction.Updated, message.MovieInfo.Movie.Id);
        }

        [NonAction]
        public void Handle(MovieFileDeletedEvent message)
        {
            BroadcastResourceChange(ModelAction.Updated, message.MovieFile.Movie.Id);
        }
    }
}
