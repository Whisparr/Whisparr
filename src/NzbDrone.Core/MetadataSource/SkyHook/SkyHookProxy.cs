using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Newtonsoft.Json;
using NLog;
using NzbDrone.Common.Cloud;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Exceptions;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.MetadataSource.SkyHook.Resource;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Movies.AlternativeTitles;
using NzbDrone.Core.Movies.Performers;
using NzbDrone.Core.Movies.Studios;
using NzbDrone.Core.Parser;

namespace NzbDrone.Core.MetadataSource.SkyHook
{
    public class SkyHookProxy : IProvideMovieInfo, ISearchForNewMovie
    {
        private readonly IHttpClient _httpClient;
        private readonly Logger _logger;

        private readonly IHttpRequestBuilderFactory _whisparrMetadata;
        private readonly IConfigService _configService;
        private readonly IMovieService _movieService;
        private readonly IMovieMetadataService _movieMetadataService;

        public SkyHookProxy(IHttpClient httpClient,
            IWhisparrCloudRequestBuilder requestBuilder,
            IConfigService configService,
            IMovieService movieService,
            IMovieMetadataService movieMetadataService,
            Logger logger)
        {
            _httpClient = httpClient;
            _whisparrMetadata = requestBuilder.WhisparrMetadata;
            _configService = configService;
            _movieService = movieService;
            _movieMetadataService = movieMetadataService;

            _logger = logger;
        }

        public HashSet<string> GetChangedMovies(DateTime startTime)
        {
            // Round down to the hour to ensure we cover gap and don't kill cache every call
            var cacheAdjustedStart = startTime.AddMinutes(-15);
            var startDate = cacheAdjustedStart.Date.AddHours(cacheAdjustedStart.Hour).ToString("s");

            var request = _whisparrMetadata.Create()
                .SetSegment("route", "movie/changed")
                .AddQueryParam("since", startDate)
                .Build();

            request.AllowAutoRedirect = true;
            request.SuppressHttpError = true;

            var response = _httpClient.Get<List<string>>(request);

            return new HashSet<string>(response.Resource);
        }

        public Tuple<MovieMetadata, Studio, List<Performer>> GetMovieInfo(int tmdbId)
        {
            var httpRequest = _whisparrMetadata.Create()
                                             .SetSegment("route", "movie")
                                             .Resource(tmdbId.ToString())
                                             .Build();

            httpRequest.AllowAutoRedirect = true;
            httpRequest.SuppressHttpError = true;

            var httpResponse = _httpClient.Get<MovieResource>(httpRequest);

            if (httpResponse.HasHttpError)
            {
                if (httpResponse.StatusCode == HttpStatusCode.NotFound)
                {
                    throw new MovieNotFoundException(tmdbId);
                }
                else
                {
                    throw new HttpException(httpRequest, httpResponse);
                }
            }

            var movie = MapMovie(httpResponse.Resource);

            movie.Credits.AddRange(httpResponse.Resource.Credits.Select(MapCast));

            var performers = httpResponse.Resource.Credits.Select(c => MapPerformer(c.Performer)).DistinctBy(p => p.ForeignId).ToList();

            return new Tuple<MovieMetadata, Studio, List<Performer>>(movie, null, performers);
        }

        public Tuple<MovieMetadata, Studio, List<Performer>> GetSceneInfo(string stashId)
        {
            var httpRequest = _whisparrMetadata.Create()
                                             .SetSegment("route", "scene")
                                             .Resource(stashId)
                                             .Build();

            httpRequest.AllowAutoRedirect = true;
            httpRequest.SuppressHttpError = true;

            var httpResponse = _httpClient.Get<MovieResource>(httpRequest);

            if (httpResponse.HasHttpError)
            {
                if (httpResponse.StatusCode == HttpStatusCode.NotFound)
                {
                    throw new MovieNotFoundException(stashId);
                }
                else
                {
                    throw new HttpException(httpRequest, httpResponse);
                }
            }

            var movie = MapMovie(httpResponse.Resource);

            movie.Credits.AddRange(httpResponse.Resource.Credits.Select(c => MapSceneCast(c, movie.ForeignId)));

            var performers = httpResponse.Resource.Credits.Select(c => MapPerformer(c.Performer)).DistinctBy(p => p.ForeignId).ToList();

            return new Tuple<MovieMetadata, Studio, List<Performer>>(movie, MapStudio(httpResponse.Resource.Studio), performers);
        }

        public List<MovieMetadata> GetBulkMovieInfo(List<int> tmdbIds)
        {
            var httpRequest = _whisparrMetadata.Create()
                                             .SetSegment("route", "movie/bulk")
                                             .Build();

            httpRequest.Headers.ContentType = "application/json";

            httpRequest.SetContent(tmdbIds.ToJson());
            httpRequest.ContentSummary = tmdbIds.ToJson(Formatting.None);

            httpRequest.AllowAutoRedirect = true;
            httpRequest.SuppressHttpError = true;

            var httpResponse = _httpClient.Post<List<MovieResource>>(httpRequest);

            if (httpResponse.HasHttpError || httpResponse.Resource.Count == 0)
            {
                throw new HttpException(httpRequest, httpResponse);
            }

            var movies = httpResponse.Resource.Select(MapMovie).ToList();

            return movies;
        }

        public MovieMetadata GetMovieByImdbId(string imdbId)
        {
            imdbId = Parser.Parser.NormalizeImdbId(imdbId);

            if (imdbId == null)
            {
                return null;
            }

            var httpRequest = _whisparrMetadata.Create()
                                             .SetSegment("route", "movie/imdb")
                                             .Resource(imdbId.ToString())
                                             .Build();

            httpRequest.AllowAutoRedirect = true;
            httpRequest.SuppressHttpError = true;

            var httpResponse = _httpClient.Get<List<MovieResource>>(httpRequest);

            if (httpResponse.HasHttpError)
            {
                if (httpResponse.StatusCode == HttpStatusCode.NotFound)
                {
                    throw new MovieNotFoundException(imdbId);
                }
                else
                {
                    throw new HttpException(httpRequest, httpResponse);
                }
            }

            var movie = httpResponse.Resource.SelectList(MapMovie).FirstOrDefault();

            return movie;
        }

        public List<string> GetPerformerScenes(string stashId)
        {
            var httpRequest = _whisparrMetadata.Create()
                                             .SetSegment("route", "performer")
                                             .Resource($"{stashId}\\works")
                                             .Build();

            httpRequest.AllowAutoRedirect = true;
            httpRequest.SuppressHttpError = true;

            var httpResponse = _httpClient.Get<PerformerWorksResource>(httpRequest);
            var scenes = httpResponse.Resource.Scenes;

            if (httpResponse.HasHttpError)
            {
                if (httpResponse.StatusCode == HttpStatusCode.NotFound)
                {
                    throw new MovieNotFoundException(stashId);
                }
                else
                {
                    throw new HttpException(httpRequest, httpResponse);
                }
            }

            return scenes;
        }

        public List<string> GetStudioScenes(string stashId)
        {
            var httpRequest = _whisparrMetadata.Create()
                                             .SetSegment("route", "site")
                                             .Resource($"{stashId}\\scenes")
                                             .Build();

            httpRequest.AllowAutoRedirect = true;
            httpRequest.SuppressHttpError = true;

            var httpResponse = _httpClient.Get<List<string>>(httpRequest);
            var scenes = httpResponse.Resource;

            if (httpResponse.HasHttpError)
            {
                if (httpResponse.StatusCode == HttpStatusCode.NotFound)
                {
                    throw new MovieNotFoundException(stashId);
                }
                else
                {
                    throw new HttpException(httpRequest, httpResponse);
                }
            }

            return scenes;
        }

        public MovieMetadata MapMovie(MovieResource resource)
        {
            var movie = new MovieMetadata();
            var altTitles = new List<AlternativeTitle>();

            movie.ItemType = resource.ItemType;
            movie.MetadataSource = resource.ItemType == ItemType.Movie ? Movies.MetadataSource.Tmdb : Movies.MetadataSource.Stash;

            movie.ForeignId = resource.ItemType == ItemType.Movie ? resource.ForeignIds.TmdbId.ToString() : resource.ForeignIds.StashId;
            movie.TmdbId = resource.ForeignIds.TmdbId;
            movie.StashId = resource.ForeignIds.StashId;
            movie.Title = resource.Title;
            movie.CleanTitle = resource.Title.CleanMovieTitle();
            movie.SortTitle = Parser.Parser.NormalizeTitle(resource.Title);
            movie.Overview = resource.Overview;

            movie.Website = resource.Homepage;
            movie.ReleaseDate = resource.ReleaseDateUtc;

            movie.Year = resource.Year;

            if (resource.Duration != null)
            {
                movie.Runtime = resource.Duration.Value;
            }

            movie.Ratings = MapRatings(resource.Ratings) ?? new Ratings();

            movie.Genres = resource.Genres;
            movie.Images = resource.Images.Select(MapImage).ToList();

            movie.ItemType = resource.ItemType;

            var now = DateTime.Now;

            movie.Status = MovieStatusType.Announced;

            if (resource.ReleaseDateUtc.HasValue && now > resource.ReleaseDateUtc)
            {
                movie.Status = MovieStatusType.Released;
            }

            if (resource.Studio.ForeignIds != null && resource.Studio.ForeignIds.StashId.IsNotNullOrWhiteSpace())
            {
                movie.StudioForeignId = resource.Studio.ForeignIds.StashId;
                movie.StudioTitle = resource.Studio.Title;
            }
            else if (resource.Studio.ForeignIds != null && resource.Studio.ForeignIds.TmdbId > 0)
            {
                movie.StudioForeignId = resource.Studio.ForeignIds.TmdbId.ToString();
                movie.StudioTitle = resource.Studio.Title;
            }

            return movie;
        }

        private string StripTrailingTheFromTitle(string title)
        {
            if (title.EndsWith(",the"))
            {
                title = title.Substring(0, title.Length - 4);
            }
            else if (title.EndsWith(", the"))
            {
                title = title.Substring(0, title.Length - 5);
            }

            return title;
        }

        public MovieMetadata MapMovieToTmdbMovie(MovieMetadata movie)
        {
            try
            {
                var newMovie = movie;

                if (movie.TmdbId > 0)
                {
                    newMovie = _movieMetadataService.FindByTmdbId(movie.TmdbId);

                    if (newMovie != null)
                    {
                        return newMovie;
                    }

                    newMovie = GetMovieInfo(movie.TmdbId).Item1;
                }
                else if (movie.ImdbId.IsNotNullOrWhiteSpace())
                {
                    newMovie = _movieMetadataService.FindByImdbId(Parser.Parser.NormalizeImdbId(movie.ImdbId));

                    if (newMovie != null)
                    {
                        return newMovie;
                    }

                    newMovie = GetMovieByImdbId(movie.ImdbId);
                }
                else
                {
                    var yearStr = "";
                    if (movie.Year > 1900)
                    {
                        yearStr = $" {movie.Year}";
                    }

                    var newMovieObject = SearchForNewMovie(movie.Title + yearStr).FirstOrDefault();

                    if (newMovieObject == null)
                    {
                        newMovie = null;
                    }
                    else
                    {
                        newMovie = newMovieObject.MovieMetadata;
                    }
                }

                if (newMovie == null)
                {
                    _logger.Warn("Couldn't map movie {0} to a movie on The Movie DB. It will not be added :(", movie.Title);
                    return null;
                }

                return newMovie;
            }
            catch (Exception ex)
            {
                _logger.Warn(ex, "Couldn't map movie {0} to a movie on The Movie DB. It will not be added :(", movie.Title);
                return null;
            }
        }

        public List<object> SearchForNewEntity(string title, ItemType itemType)
        {
            var lowerTitle = title.ToLower();
            var result = new List<object>();

            if (lowerTitle.StartsWith("tmdb:") || lowerTitle.StartsWith("tmdbid:") || lowerTitle.StartsWith("imdb:") || lowerTitle.StartsWith("imdbid:") || lowerTitle.StartsWith("stash:") || lowerTitle.StartsWith("stashId:"))
            {
                var movies = itemType == ItemType.Movie ? SearchForNewMovie(title) : SearchForNewScene(title);

                foreach (var movie in movies)
                {
                    result.Add(movie);
                }
            }
            else
            {
                var searchTerm = lowerTitle.Replace("_", " ").Replace(".", " ");

                var route = itemType == ItemType.Movie ? "movie/search" : "scene/search";

                var request = _whisparrMetadata.Create()
                    .SetSegment("route", route)
                    .AddQueryParam("q", searchTerm)
                    .Build();

                request.AllowAutoRedirect = true;
                request.SuppressHttpError = true;

                var httpResponse = _httpClient.Get<List<MovieResource>>(request);

                var performersAdded = new List<string>();

                foreach (var movie in httpResponse.Resource)
                {
                    foreach (var performer in movie.Credits)
                    {
                        if (performer.Performer.Name.ToLower().Contains(lowerTitle))
                        {
                            var mappedPerformer = MapPerformer(performer.Performer);

                            if (mappedPerformer.ForeignId.IsNotNullOrWhiteSpace() && !performersAdded.Contains(mappedPerformer.ForeignId.ToLower()))
                            {
                                performersAdded.Add(mappedPerformer.ForeignId.ToLower());
                                result.Add(MapPerformer(performer.Performer));
                            }
                        }
                    }

                    result.Add(MapSearchResult(movie));
                }
            }

            return result;
        }

        public List<Movie> SearchForNewMovie(string title)
        {
            try
            {
                var lowerTitle = title.ToLower();

                lowerTitle = lowerTitle.Replace(".", "");

                var parserTitle = lowerTitle;

                var parserResult = Parser.Parser.ParseMovieTitle(title, true);

                var yearTerm = "";

                if (parserResult != null && parserResult.PrimaryMovieTitle != title)
                {
                    // Parser found something interesting!
                    parserTitle = parserResult.PrimaryMovieTitle.ToLower().Replace(".", " "); // TODO Update so not every period gets replaced (e.g. R.I.P.D.)
                    if (parserResult.Year > 1800)
                    {
                        yearTerm = parserResult.Year.ToString();
                    }

                    if (parserResult.ImdbId.IsNotNullOrWhiteSpace())
                    {
                        try
                        {
                            var movieLookup = GetMovieByImdbId(parserResult.ImdbId);
                            return movieLookup == null ? new List<Movie>() : new List<Movie> { _movieService.FindByTmdbId(movieLookup.TmdbId) ?? new Movie { MovieMetadata = movieLookup } };
                        }
                        catch (Exception)
                        {
                            return new List<Movie>();
                        }
                    }

                    if (parserResult.TmdbId > 0)
                    {
                        try
                        {
                            var movieLookup = GetMovieInfo(parserResult.TmdbId).Item1;
                            return movieLookup == null ? new List<Movie>() : new List<Movie> { _movieService.FindByTmdbId(movieLookup.TmdbId) ?? new Movie { MovieMetadata = movieLookup } };
                        }
                        catch (Exception)
                        {
                            return new List<Movie>();
                        }
                    }
                }

                parserTitle = StripTrailingTheFromTitle(parserTitle);

                if (lowerTitle.StartsWith("imdb:") || lowerTitle.StartsWith("imdbid:"))
                {
                    var slug = lowerTitle.Split(':')[1].Trim();

                    var imdbid = slug;

                    if (slug.IsNullOrWhiteSpace() || slug.Any(char.IsWhiteSpace))
                    {
                        return new List<Movie>();
                    }

                    try
                    {
                        var movieLookup = GetMovieByImdbId(imdbid);
                        return movieLookup == null ? new List<Movie>() : new List<Movie> { _movieService.FindByTmdbId(movieLookup.TmdbId) ?? new Movie { MovieMetadata = movieLookup } };
                    }
                    catch (MovieNotFoundException)
                    {
                        return new List<Movie>();
                    }
                }

                if (lowerTitle.StartsWith("tmdb:") || lowerTitle.StartsWith("tmdbid:"))
                {
                    var slug = lowerTitle.Split(':')[1].Trim();

                    var tmdbid = -1;

                    if (slug.IsNullOrWhiteSpace() || slug.Any(char.IsWhiteSpace) || !int.TryParse(slug, out tmdbid))
                    {
                        return new List<Movie>();
                    }

                    try
                    {
                        var movieLookup = GetMovieInfo(tmdbid).Item1;
                        return movieLookup == null ? new List<Movie>() : new List<Movie> { _movieService.FindByTmdbId(movieLookup.TmdbId) ?? new Movie { MovieMetadata = movieLookup } };
                    }
                    catch (MovieNotFoundException)
                    {
                        return new List<Movie>();
                    }
                }

                var searchTerm = parserTitle.Replace("_", " ").Replace(".", " ");

                var firstChar = searchTerm.First();

                var request = _whisparrMetadata.Create()
                    .SetSegment("route", "movie/search")
                    .AddQueryParam("q", searchTerm)
                    .AddQueryParam("year", yearTerm)
                    .Build();

                request.AllowAutoRedirect = true;
                request.SuppressHttpError = true;

                var httpResponse = _httpClient.Get<List<MovieResource>>(request);

                return httpResponse.Resource.SelectList(MapSearchResult);
            }
            catch (HttpException ex)
            {
                _logger.Warn(ex);
                throw new SkyHookException("Search for '{0}' failed. Unable to communicate with TMDb.", ex, title);
            }
            catch (WebException ex)
            {
                _logger.Warn(ex);
                throw new SkyHookException("Search for '{0}' failed. Unable to communicate with TMDb.", ex, title, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.Warn(ex);
                throw new SkyHookException("Search for '{0}' failed. Invalid response received from TMDb.", ex, title);
            }
        }

        public List<Movie> SearchForNewScene(string title)
        {
            try
            {
                var lowerTitle = title.ToLower();

                lowerTitle = lowerTitle.Replace(".", "");

                if (lowerTitle.StartsWith("stash:") || lowerTitle.StartsWith("stashid:"))
                {
                    var slug = lowerTitle.Split(':')[1].Trim();

                    var stashId = slug;

                    if (slug.IsNullOrWhiteSpace() || slug.Any(char.IsWhiteSpace))
                    {
                        return new List<Movie>();
                    }

                    try
                    {
                        var movieLookup = GetSceneInfo(stashId).Item1;
                        return movieLookup == null ? new List<Movie>() : new List<Movie> { _movieService.FindByForeignId(movieLookup.StashId) ?? new Movie { MovieMetadata = movieLookup } };
                    }
                    catch (MovieNotFoundException)
                    {
                        return new List<Movie>();
                    }
                }

                var searchTerm = lowerTitle.Replace("_", " ").Replace(".", " ");

                var firstChar = searchTerm.First();

                var request = _whisparrMetadata.Create()
                    .SetSegment("route", "scene/search")
                    .AddQueryParam("q", searchTerm)
                    .Build();

                request.AllowAutoRedirect = true;
                request.SuppressHttpError = true;

                var httpResponse = _httpClient.Get<List<MovieResource>>(request);

                return httpResponse.Resource.SelectList(MapSearchResult);
            }
            catch (HttpException ex)
            {
                _logger.Warn(ex);
                throw new SkyHookException("Search for '{0}' failed. Unable to communicate with StashDb.", ex, title);
            }
            catch (WebException ex)
            {
                _logger.Warn(ex);
                throw new SkyHookException("Search for '{0}' failed. Unable to communicate with StashDb.", ex, title, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.Warn(ex);
                throw new SkyHookException("Search for '{0}' failed. Invalid response received from StashDb.", ex, title);
            }
        }

        private Movie MapSearchResult(MovieResource result)
        {
            var movie = _movieService.FindByForeignId(result.ItemType == ItemType.Movie ? result.ForeignIds.TmdbId.ToString() : result.ForeignIds.StashId);

            if (movie == null)
            {
                movie = new Movie { MovieMetadata = MapMovie(result) };
            }

            return movie;
        }

        private static Credit MapCast(CastResource arg)
        {
            var newActor = new Credit
            {
                Character = arg.Character,
                Order = arg.Order,
                CreditForeignId = arg.CreditId,
                Type = CreditType.Cast,
                Performer = new CreditPerformer
                {
                    Name = arg.Performer.Name,
                    ForeignId = arg.Performer.ForeignIds.TmdbId.ToString(),
                    Images = arg.Performer.Images.Select(MapImage).ToList()
                }
            };

            return newActor;
        }

        private Performer MapPerformer(PerformerResource performer)
        {
            var newPerformer = new Performer
            {
                Name = performer.Name,
                Gender = MapGender(performer.Gender),
                ForeignId = performer.ForeignIds.StashId,
                Images = performer.Images.Select(MapImage).ToList()
            };

            return newPerformer;
        }

        private Gender MapGender(string gender)
        {
            if (gender.IsNullOrWhiteSpace())
            {
                return Gender.Female;
            }

            switch (gender.ToUpperInvariant())
            {
                case "TRANSGENDER_FEMALE":
                    return Gender.TransFemale;
                case "TRANSGENDER_MALE":
                    return Gender.TransMale;
                case "NON_BINARY":
                    return Gender.NonBinary;
                case "INTERSEX":
                    return Gender.Intersex;
                case "MALE":
                    return Gender.Male;
                default:
                    return Gender.Female;
            }
        }

        private Studio MapStudio(StudioResource studio)
        {
            var newPerformer = new Studio
            {
                Title = studio.Title,
                Website = studio.Homepage,
                ForeignId = studio.ForeignIds.StashId,
                Images = studio.Images?.Select(MapImage).ToList() ?? new List<MediaCover.MediaCover>()
            };

            return newPerformer;
        }

        private static Credit MapSceneCast(CastResource arg, string sceneForeignId)
        {
            var newActor = new Credit
            {
                Character = arg.Character,
                Order = arg.Order,
                CreditForeignId = $"{sceneForeignId} - {arg.Performer.ForeignIds.StashId}",
                Type = CreditType.Cast,
                Performer = new CreditPerformer
                {
                    Name = arg.Performer.Name,
                    ForeignId = arg.Performer.ForeignIds.StashId,
                    Images = arg.Performer.Images?.Select(MapImage).ToList() ?? new List<MediaCover.MediaCover>()
                }
            };

            return newActor;
        }

        private static Ratings MapRatings(RatingResource ratings)
        {
            if (ratings == null)
            {
                return new Ratings();
            }

            var mappedRatings = new Ratings();

            if (ratings.Tmdb != null)
            {
                mappedRatings.Tmdb = new RatingChild
                {
                    Type = RatingType.User,
                    Value = ratings.Tmdb.Value,
                    Votes = ratings.Tmdb.Count
                };
            }

            return mappedRatings;
        }

        private static MediaCover.MediaCover MapImage(ImageResource arg)
        {
            return new MediaCover.MediaCover
            {
                RemoteUrl = arg.Url,
                CoverType = MapCoverType(arg.CoverType)
            };
        }

        private static MediaCoverTypes MapCoverType(string coverType)
        {
            switch (coverType.ToLower())
            {
                case "poster":
                    return MediaCoverTypes.Poster;
                case "headshot":
                    return MediaCoverTypes.Headshot;
                case "fanart":
                    return MediaCoverTypes.Fanart;
                case "clearlogo":
                    return MediaCoverTypes.Clearlogo;
                case "screenshot":
                    return MediaCoverTypes.Screenshot;
                default:
                    return MediaCoverTypes.Unknown;
            }
        }
    }
}
