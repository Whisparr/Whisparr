using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Movies.Credits;
using Whisparr.Http;
using Whisparr.Http.REST;

namespace NzbDrone.Api.V3.Credits
{
    [V3ApiController]
    public class CreditController : RestController<CreditResource>
    {
        private readonly ICreditService _creditService;
        private readonly IMovieService _movieService;
        private readonly IMapCoversToLocal _coverMapper;

        public CreditController(ICreditService creditService, IMovieService movieService, IMapCoversToLocal coverMapper)
        {
            _creditService = creditService;
            _movieService = movieService;
            _coverMapper = coverMapper;
        }

        protected override CreditResource GetResourceById(int id)
        {
            return _creditService.GetById(id).ToResource();
        }

        [HttpGet]
        public List<CreditResource> GetCredits(int? movieId, int? movieMetadataId, string performerId)
        {
            if (movieMetadataId.HasValue)
            {
                return MapToResource(_creditService.GetAllCreditsForMovieMetadata(movieMetadataId.Value)).ToList();
            }

            if (movieId.HasValue)
            {
                var movie = _movieService.GetMovie(movieId.Value);

                return MapToResource(_creditService.GetAllCreditsForMovieMetadata(movie.MovieMetadataId)).ToList();
            }

            if (performerId.IsNotNullOrWhiteSpace())
            {
                return MapToResource(_creditService.GetPerformerMovies(performerId)).ToList();
            }

            return MapToResource(_creditService.GetAllCredits()).ToList();
        }

        private IEnumerable<CreditResource> MapToResource(IEnumerable<Credit> credits)
        {
            foreach (var currentCredits in credits)
            {
                var resource = currentCredits.ToResource();
                if (resource.Images.Any())
                {
                    _coverMapper.ConvertToLocalUrls(0, resource.Images);
                }

                yield return resource;
            }
        }
    }
}
