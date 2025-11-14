using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.Movies;
using NzbDrone.Core.MovieStats;
using NzbDrone.Core.Tags;
using NzbDrone.SignalR;
using Whisparr.Api.V3.Movies;
using Whisparr.Http;

namespace Whisparr.Api.V3.Calendar
{
    [V3ApiController]
    public class CalendarController : MovieControllerWithSignalR
    {
        private readonly IMovieService _moviesService;
        private new readonly IMovieStatisticsService _movieStatisticsService;
        private new readonly IUpgradableSpecification _upgradableSpecification;
        private readonly ITagService _tagService;

        public CalendarController(IBroadcastSignalRMessage signalR,
                            IMovieService moviesService,
                            IMovieStatisticsService movieStatisticsService,
                            IUpgradableSpecification upgradableSpecification,
                            ICustomFormatCalculationService formatCalculator,
                            ITagService tagService,
                            IConfigService configService)
            : base(moviesService, movieStatisticsService, upgradableSpecification, formatCalculator, configService, signalR)
        {
            _moviesService = moviesService;
            _movieStatisticsService = movieStatisticsService;
            _upgradableSpecification = upgradableSpecification;
            _tagService = tagService;
        }

        [NonAction]
        protected override MovieResource GetResourceById(int id)
        {
            throw new NotImplementedException();
        }

        [HttpGet]
        [Produces("application/json")]
        public List<MovieResource> GetCalendar(DateTime? start, DateTime? end, bool unmonitored = false, string tags = "")
        {
            var startUse = start ?? DateTime.Today;
            var endUse = end ?? DateTime.Today.AddDays(2);
            var movies = _moviesService.GetMoviesBetweenDates(startUse, endUse, unmonitored);
            var parsedTags = new List<int>();
            var results = new List<Movie>();

            if (tags.IsNotNullOrWhiteSpace())
            {
                parsedTags.AddRange(tags.Split(',').Select(_tagService.GetTag).Select(t => t.Id));
            }

            foreach (var movie in movies)
            {
                if (movie == null)
                {
                    continue;
                }

                if (parsedTags.Any() && parsedTags.None(movie.Tags.Contains))
                {
                    continue;
                }

                results.Add(movie);
            }

            var resources = MapToResource(results);

            return resources.OrderBy(e => e.ReleaseDate).ToList();
        }

        protected new List<MovieResource> MapToResource(List<Movie> movies)
        {
            var resources = new List<MovieResource>();
            var availDelay = _configService.AvailabilityDelay;

            foreach (var movie in movies)
            {
                if (movie == null)
                {
                    continue;
                }

                var resource = movie.ToResource(availDelay, _upgradableSpecification);
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
    }
}
