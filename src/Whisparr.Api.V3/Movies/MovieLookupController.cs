using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.SeriesStats;
using Whisparr.Http;

namespace Whisparr.Api.V3.Movies
{
    [V3ApiController("movie/lookup")]
    public class MovieLookupController : Controller
    {
        private readonly ISearchForNewMovies _searchProxy;
        private readonly IBuildFileNames _fileNameBuilder;
        private readonly IMapCoversToLocal _coverMapper;

        public MovieLookupController(ISearchForNewMovies searchProxy, IBuildFileNames fileNameBuilder, IMapCoversToLocal coverMapper)
        {
            _searchProxy = searchProxy;
            _fileNameBuilder = fileNameBuilder;
            _coverMapper = coverMapper;
        }

        [HttpGet]
        public object Search([FromQuery] string term)
        {
            var tmdbResults = _searchProxy.SearchForNewMovies(term);
            return MapToResource(tmdbResults);
        }

        private IEnumerable<MovieResource> MapToResource(IEnumerable<NzbDrone.Core.Movies.Movie> movies)
        {
            foreach (var currentMovie in movies)
            {
                var resource = currentMovie.ToResource();

                _coverMapper.ConvertToLocalUrls(resource.Id, MediaCoverEntity.Movie, resource.Images);

                var poster = currentMovie.Images.FirstOrDefault(c => c.CoverType == MediaCoverTypes.Poster);

                if (poster != null)
                {
                    resource.RemotePoster = poster.RemoteUrl;
                }

                resource.Folder = _fileNameBuilder.GetMovieFolder(currentMovie);
                resource.Statistics = new MovieStatistics().ToResource();

                yield return resource;
            }
        }
    }
}
