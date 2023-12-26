using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Languages;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Organizer;
using Whisparr.Http;
using Whisparr.Http.REST;

namespace Whisparr.Api.V3.Movies
{
    [V3ApiController("movie/lookup")]
    public class MovieLookupController : RestController<MovieResource>
    {
        private readonly ISearchForNewMovie _searchProxy;
        private readonly IProvideMovieInfo _movieInfo;
        private readonly IBuildFileNames _fileNameBuilder;
        private readonly INamingConfigService _namingService;
        private readonly IMapCoversToLocal _coverMapper;
        private readonly IConfigService _configService;

        public MovieLookupController(ISearchForNewMovie searchProxy,
                                 IProvideMovieInfo movieInfo,
                                 IBuildFileNames fileNameBuilder,
                                 INamingConfigService namingService,
                                 IMapCoversToLocal coverMapper,
                                 IConfigService configService)
        {
            _movieInfo = movieInfo;
            _searchProxy = searchProxy;
            _fileNameBuilder = fileNameBuilder;
            _namingService = namingService;
            _coverMapper = coverMapper;
            _configService = configService;
        }

        [NonAction]
        protected override MovieResource GetResourceById(int id)
        {
            throw new NotImplementedException();
        }

        [HttpGet("tmdb")]
        public object SearchByTmdbId(int tmdbId)
        {
            var availDelay = _configService.AvailabilityDelay;
            var result = new Movie { MovieMetadata = _movieInfo.GetMovieInfo(tmdbId) };
            return result.ToResource(availDelay);
        }

        [HttpGet("imdb")]
        public object SearchByImdbId(string imdbId)
        {
            var result = new Movie { MovieMetadata = _movieInfo.GetMovieByImdbId(imdbId) };

            var availDelay = _configService.AvailabilityDelay;
            return result.ToResource(availDelay);
        }

        [HttpGet]
        public object Search([FromQuery] string term)
        {
            var searchResults = _searchProxy.SearchForNewMovie(term);

            return MapToResource(searchResults);
        }

        private IEnumerable<MovieResource> MapToResource(IEnumerable<Movie> movies)
        {
            var movieInfoLanguage = (Language)_configService.MovieInfoLanguage;
            var availDelay = _configService.AvailabilityDelay;
            var namingConfig = _namingService.GetConfig();

            foreach (var currentMovie in movies)
            {
                var resource = currentMovie.ToResource(availDelay);

                _coverMapper.ConvertToLocalUrls(resource.Id, resource.Images);

                var poster = currentMovie.MovieMetadata.Value.Images.FirstOrDefault(c => c.CoverType == MediaCoverTypes.Poster);
                if (poster != null)
                {
                    resource.RemotePoster = poster.RemoteUrl;
                }

                resource.Folder = _fileNameBuilder.GetMovieFolder(currentMovie, namingConfig);

                yield return resource;
            }
        }
    }
}
