using System;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.Movies;
using NzbDrone.Core.MovieStats;
using NzbDrone.SignalR;
using Whisparr.Api.V3.Movies;
using Whisparr.Http;
using Whisparr.Http.Extensions;

namespace Whisparr.Api.V3.Wanted
{
    [V3ApiController("wanted/missing")]
    public class MissingController : MovieControllerWithSignalR
    {
        public MissingController(IMovieService movieService,
                            IMovieStatisticsService movieStatisticsService,
                            IUpgradableSpecification upgradableSpecification,
                            ICustomFormatCalculationService formatCalculator,
                            IConfigService configService,
                            IBroadcastSignalRMessage signalRBroadcaster)
            : base(movieService, movieStatisticsService, upgradableSpecification, formatCalculator, configService, signalRBroadcaster)
        {
        }

        [NonAction]
        protected override MovieResource GetResourceById(int id)
        {
            throw new NotImplementedException();
        }

        [HttpGet]
        [Produces("application/json")]
        public PagingResource<MovieResource> GetMissingMovies([FromQuery] PagingRequestResource paging, bool monitored = true)
        {
            var pagingResource = new PagingResource<MovieResource>(paging);
            var pagingSpec = new PagingSpec<Movie>
            {
                Page = pagingResource.Page,
                PageSize = pagingResource.PageSize,
                SortKey = pagingResource.SortKey,
                SortDirection = pagingResource.SortDirection
            };

            pagingSpec.FilterExpressions.Add(v => v.Monitored == monitored);

            var resource = pagingSpec.ApplyToPage(_movieService.MoviesWithoutFiles, v => MapToResource(v));

            return resource;
        }
    }
}
