using System.Collections.Generic;
using Newtonsoft.Json;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.ImportLists.ImportListMovies;

namespace NzbDrone.Core.ImportLists.TMDb.Company
{
    public class TMDbCompanyParser : TMDbParser
    {
        public override IList<ImportListMovie> ParseResponse(ImportListResponse importResponse)
        {
            var movies = new List<ImportListMovie>();

            if (!PreProcess(importResponse))
            {
                return movies;
            }

            var jsonResponse = JsonConvert.DeserializeObject<MovieSearchResource>(importResponse.Content);

            // no movies were return
            if (jsonResponse == null)
            {
                return movies;
            }

            foreach (var movie in jsonResponse.Results)
            {
                // Movies with no Year Fix
                if (string.IsNullOrWhiteSpace(movie.ReleaseDate) || !movie.Adult)
                {
                    continue;
                }

                movies.AddIfNotNull(MapListMovie(movie));
            }

            return movies;
        }
    }
}
