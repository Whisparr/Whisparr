using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Movies.Performers;
using NzbDrone.Core.Organizer;
using Whisparr.Api.V3.Movies;
using Whisparr.Api.V3.Performers;
using Whisparr.Api.V3.Search;
using Whisparr.Http;

namespace Readarr.Api.V1.Search
{
    [V3ApiController("lookup")]
    public class SearchController : Controller
    {
        private readonly ISearchForNewMovie _searchProxy;
        private readonly IBuildFileNames _fileNameBuilder;
        private readonly INamingConfigService _namingService;
        private readonly IMapCoversToLocal _coverMapper;
        private readonly IConfigService _configService;

        public SearchController(ISearchForNewMovie searchProxy,
                                IBuildFileNames fileNameBuilder,
                                INamingConfigService namingService,
                                IMapCoversToLocal coverMapper,
                                IConfigService configService)
        {
            _searchProxy = searchProxy;
            _fileNameBuilder = fileNameBuilder;
            _namingService = namingService;
            _coverMapper = coverMapper;
            _configService = configService;
        }

        [HttpGet("scene")]
        public object SearchScene([FromQuery] string term)
        {
            var searchResults = _searchProxy.SearchForNewEntity(term, ItemType.Scene);
            return MapToResource(searchResults).ToList();
        }

        [HttpGet("movie")]
        public object SearchMovie([FromQuery] string term)
        {
            var searchResults = _searchProxy.SearchForNewEntity(term, ItemType.Movie);
            return MapToResource(searchResults).ToList();
        }

        private IEnumerable<SearchResource> MapToResource(IEnumerable<object> results)
        {
            var id = 1;
            var availDelay = _configService.AvailabilityDelay;
            var namingConfig = _namingService.GetConfig();

            foreach (var result in results)
            {
                var resource = new SearchResource();
                resource.Id = id++;

                if (result is Movie)
                {
                    var movie = (Movie)result;
                    var movieResource = movie.ToResource(availDelay);

                    _coverMapper.ConvertToLocalUrls(movieResource.Id, movieResource.Images);

                    var poster = movie.MovieMetadata.Value.Images.FirstOrDefault(c => c.CoverType == MediaCoverTypes.Poster);
                    if (poster != null)
                    {
                        movieResource.RemotePoster = poster.RemoteUrl;
                    }

                    movieResource.Folder = _fileNameBuilder.GetMovieFolder(movie, namingConfig);
                    resource.Movie = movieResource;
                    resource.ForeignId = movie.ForeignId;
                }
                else if (result is Performer)
                {
                    var performer = (Performer)result;
                    var performerResource = performer.ToResource();
                    _coverMapper.ConvertToLocalUrls(performerResource.Id, performerResource.Images);

                    var cover = performer.Images.FirstOrDefault(c => c.CoverType == MediaCoverTypes.Headshot);
                    if (cover != null)
                    {
                        performerResource.RemotePoster = cover.RemoteUrl;
                    }

                    resource.Performer = performerResource;
                    resource.ForeignId = performer.ForeignId;
                }
                else
                {
                    throw new NotImplementedException("Bad response from search all proxy");
                }

                yield return resource;
            }
        }
    }
}
