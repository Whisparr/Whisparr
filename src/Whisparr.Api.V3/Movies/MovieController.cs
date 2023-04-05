using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.Movies;
using NzbDrone.Core.RootFolders;
using NzbDrone.Core.SeriesStats;
using NzbDrone.Core.Validation;
using NzbDrone.Core.Validation.Paths;
using NzbDrone.SignalR;
using Whisparr.Http;
using Whisparr.Http.REST;
using Whisparr.Http.REST.Attributes;

namespace Whisparr.Api.V3.Movies
{
    [V3ApiController]
    public class MovieController : RestControllerWithSignalR<MovieResource, Movie>
    {
        private readonly IMovieService _moviesService;
        private readonly IAddMovieService _addMovieService;
        private readonly ISeriesStatisticsService _statisticsService;
        private readonly IMapCoversToLocal _coverMapper;
        private readonly IRootFolderService _rootFolderService;

        public MovieController(IBroadcastSignalRMessage signalRBroadcaster,
                               IMovieService moviesService,
                               IAddMovieService addMovieService,
                               ISeriesStatisticsService statisticsService,
                               IMapCoversToLocal coverMapper,
                               IRootFolderService rootFolderService,
                               RootFolderValidator rootFolderValidator,
                               MappedNetworkDriveValidator mappedNetworkDriveValidator,
                               MediaPathValidator mediaPathValidator,
                               MovieExistsValidator movieExistsValidator,
                               MediaAncestorValidator mediaAncestorValidator,
                               SystemFolderValidator systemFolderValidator,
                               ProfileExistsValidator profileExistsValidator)
            : base(signalRBroadcaster)
        {
            _moviesService = moviesService;
            _addMovieService = addMovieService;
            _statisticsService = statisticsService;
            _coverMapper = coverMapper;
            _rootFolderService = rootFolderService;

            Http.Validation.RuleBuilderExtensions.ValidId(SharedValidator.RuleFor(s => s.QualityProfileId));

            SharedValidator.RuleFor(s => s.Path)
                .Cascade(CascadeMode.Stop)
                .IsValidPath()
                           .SetValidator(rootFolderValidator)
                           .SetValidator(mappedNetworkDriveValidator)
                           .SetValidator(mediaPathValidator)
                           .SetValidator(mediaAncestorValidator)
                           .SetValidator(systemFolderValidator)
                .When(s => !s.Path.IsNullOrWhiteSpace());

            SharedValidator.RuleFor(s => s.QualityProfileId).SetValidator(profileExistsValidator);

            PostValidator.RuleFor(s => s.Path).IsValidPath().When(s => s.RootFolderPath.IsNullOrWhiteSpace());
            PostValidator.RuleFor(s => s.RootFolderPath)
                         .IsValidPath()
                         .When(s => s.Path.IsNullOrWhiteSpace());
            PostValidator.RuleFor(s => s.Title).NotEmpty();
            PostValidator.RuleFor(s => s.TmdbId).GreaterThan(0).SetValidator(movieExistsValidator);

            PutValidator.RuleFor(s => s.Path).IsValidPath();
        }

        [HttpGet]
        [Produces("application/json")]
        public List<MovieResource> AllMovies(int? tmdbId)
        {
            var movieStats = _statisticsService.MovieStatistics();
            var movieResources = new List<MovieResource>();

            if (tmdbId.HasValue)
            {
                movieResources.AddIfNotNull(_moviesService.FindByTmdbId(tmdbId.Value).ToResource());
            }
            else
            {
                movieResources.AddRange(_moviesService.GetAllMovies().Select(s => s.ToResource()));
            }

            MapCoversToLocal(movieResources.ToArray());
            LinkMovieStatistics(movieResources, movieStats);
            movieResources.ForEach(LinkRootFolderPath);

            return movieResources;
        }

        protected override MovieResource GetResourceById(int id)
        {
            var movie = _moviesService.GetMovie(id);
            return GetMovieResource(movie);
        }

        [RestPostById]
        [Consumes("application/json")]
        public ActionResult<MovieResource> AddMovie(MovieResource movieResource)
        {
            var movie = _addMovieService.AddMovie(movieResource.ToModel());

            return Created(movie.Id);
        }

        private MovieResource GetMovieResource(Movie movie)
        {
            if (movie == null)
            {
                return null;
            }

            var resource = movie.ToResource();
            MapCoversToLocal(resource);
            FetchAndLinkMovieStatistics(resource);
            LinkRootFolderPath(resource);

            return resource;
        }

        private void MapCoversToLocal(params MovieResource[] series)
        {
            foreach (var seriesResource in series)
            {
                _coverMapper.ConvertToLocalUrls(seriesResource.Id, MediaCoverEntity.Movie, seriesResource.Images);
            }
        }

        private void FetchAndLinkMovieStatistics(MovieResource resource)
        {
            LinkMovieStatistics(resource, _statisticsService.MovieStatistics(resource.Id));
        }

        private void LinkMovieStatistics(List<MovieResource> resources, List<MovieStatistics> movieStatistics)
        {
            foreach (var movie in resources)
            {
                var stats = movieStatistics.SingleOrDefault(ss => ss.MovieId == movie.Id);
                if (stats == null)
                {
                    continue;
                }

                LinkMovieStatistics(movie, stats);
            }
        }

        private void LinkMovieStatistics(MovieResource resource, MovieStatistics movieStatistics)
        {
            resource.Statistics = movieStatistics.ToResource();
        }

        private void LinkRootFolderPath(MovieResource resource)
        {
            resource.RootFolderPath = _rootFolderService.GetBestRootFolderPath(resource.Path);
        }
    }
}
