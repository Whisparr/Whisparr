using Microsoft.AspNetCore.Mvc;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Organizer;
using Whisparr.Http;

namespace Whisparr.Api.V3.Movies;

[V3ApiController("movie")]
public class MovieFolderController : Controller
{
    private readonly IMovieService _movieService;
    private readonly IBuildFileNames _fileNameBuilder;

    public MovieFolderController(IMovieService movieService, IBuildFileNames fileNameBuilder)
    {
        _movieService = movieService;
        _fileNameBuilder = fileNameBuilder;
    }

    [HttpGet("{id}/folder")]
    [Produces("application/json")]
    public object GetFolder([FromRoute] int id)
    {
        var movie = _movieService.GetMovie(id);
        var folder = _fileNameBuilder.GetMovieFolder(movie);

        return new
        {
            folder
        };
    }
}
