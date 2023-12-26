using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Core.Movies.Performers;
using Whisparr.Api.V3.Movies;
using Whisparr.Api.V3.Search;
using Whisparr.Http;

namespace Readarr.Api.V1.Search
{
    [V3ApiController]
    public class SearchController : Controller
    {
        private readonly ISearchForNewEntity _searchProxy;

        public SearchController(ISearchForNewEntity searchProxy)
        {
            _searchProxy = searchProxy;
        }

        [HttpGet]
        public object Search([FromQuery] string term)
        {
            var searchResults = _searchProxy.SearchForNewEntity(term);
            return MapToResource(searchResults).ToList();
        }

        private static IEnumerable<SearchResource> MapToResource(IEnumerable<object> results)
        {
            var id = 1;
            foreach (var result in results)
            {
                var resource = new SearchResource();
                resource.Id = id++;

                if (result is NzbDrone.Core.Movies.Movie)
                {
                    var movie = (NzbDrone.Core.Movies.Movie)result;
                    resource.Movie = movie.ToResource(0);
                    resource.ForeignId = movie.ForeignId;

                    var poster = movie.MovieMetadata.Value.Images.FirstOrDefault(c => c.CoverType == MediaCoverTypes.Poster);
                    if (poster != null)
                    {
                        resource.Movie.RemotePoster = poster.Url;
                    }
                }
                else if (result is Performer)
                {
                    var performer = (Performer)result;
                    resource.Performer = performer.ToResource();
                    resource.ForeignId = performer.ForeignId;

                    var cover = performer.Images.FirstOrDefault(c => c.CoverType == MediaCoverTypes.Headshot);
                    if (cover != null)
                    {
                        resource.Performer.RemotePoster = cover.Url;
                    }
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
