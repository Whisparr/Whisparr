using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Tags;
using NzbDrone.SignalR;
using Whisparr.Api.V3.Movies;
using Whisparr.Http;
using Whisparr.Http.REST;

namespace Whisparr.Api.V3.Calendar
{
    [V3ApiController]
    public class CalendarController : RestControllerWithSignalR<MovieResource, Movie>
    {
        private readonly IMovieService _moviesService;
        private readonly IUpgradableSpecification _qualityUpgradableSpecification;
        private readonly ITagService _tagService;
        private readonly IConfigService _configService;

        public CalendarController(IBroadcastSignalRMessage signalR,
                            IMovieService moviesService,
                            IUpgradableSpecification qualityUpgradableSpecification,
                            ITagService tagService,
                            IConfigService configService)
            : base(signalR)
        {
            _moviesService = moviesService;
            _qualityUpgradableSpecification = qualityUpgradableSpecification;
            _tagService = tagService;
            _configService = configService;
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

                resources.Add(movie.ToResource(availDelay, _qualityUpgradableSpecification));
            }

            return resources;
        }
    }
}
