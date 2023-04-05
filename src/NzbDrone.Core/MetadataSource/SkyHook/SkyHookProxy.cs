using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using NLog;
using NzbDrone.Common.Cloud;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.Exceptions;
using NzbDrone.Core.Languages;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.MetadataSource.SkyHook.Resource;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.MetadataSource.SkyHook
{
    public class SkyHookProxy : IProvideSeriesInfo, IProvideMovieInfo, ISearchForNewSeries, ISearchForNewMovies
    {
        private readonly IHttpClient _httpClient;
        private readonly Logger _logger;
        private readonly ISeriesService _seriesService;
        private readonly IMovieService _movieService;
        private readonly IHttpRequestBuilderFactory _requestBuilder;

        public SkyHookProxy(IHttpClient httpClient,
                            IWhisparrCloudRequestBuilder requestBuilder,
                            ISeriesService seriesService,
                            IMovieService movieService,
                            Logger logger)
        {
            _httpClient = httpClient;
            _requestBuilder = requestBuilder.WhisparrMetadata;
            _logger = logger;
            _seriesService = seriesService;
            _movieService = movieService;
        }

        public Tuple<Series, List<Episode>> GetSeriesInfo(int tvdbSeriesId)
        {
            var httpRequest = _requestBuilder.Create()
                                             .SetSegment("route", "site")
                                             .Resource(tvdbSeriesId.ToString())
                                             .Build();

            httpRequest.AllowAutoRedirect = true;
            httpRequest.SuppressHttpError = true;

            var httpResponse = _httpClient.Get<ShowResource>(httpRequest);

            if (httpResponse.HasHttpError)
            {
                if (httpResponse.StatusCode == HttpStatusCode.NotFound)
                {
                    throw new SeriesNotFoundException(tvdbSeriesId);
                }
                else
                {
                    throw new HttpException(httpRequest, httpResponse);
                }
            }

            var episodes = httpResponse.Resource.Episodes.Select(MapEpisode);
            var series = MapSeries(httpResponse.Resource);

            return new Tuple<Series, List<Episode>>(series, episodes.ToList());
        }

        public Movie GetMovieInfo(int tmdbMovieId)
        {
            var httpRequest = _requestBuilder.Create()
                                             .SetSegment("route", "movie")
                                             .Resource(tmdbMovieId.ToString())
                                             .Build();

            httpRequest.AllowAutoRedirect = true;
            httpRequest.SuppressHttpError = true;

            var httpResponse = _httpClient.Get<MovieResource>(httpRequest);

            if (httpResponse.HasHttpError)
            {
                if (httpResponse.StatusCode == HttpStatusCode.NotFound)
                {
                    throw new SeriesNotFoundException(tmdbMovieId);
                }
                else
                {
                    throw new HttpException(httpRequest, httpResponse);
                }
            }

            var movie = MapMovie(httpResponse.Resource);

            return movie;
        }

        public List<Series> SearchForNewSeries(string title)
        {
            try
            {
                var lowerTitle = title.ToLowerInvariant();

                if (lowerTitle.StartsWith("tpdb:") || lowerTitle.StartsWith("tpdbid:"))
                {
                    var slug = lowerTitle.Split(':')[1].Trim();

                    if (slug.IsNullOrWhiteSpace() || slug.Any(char.IsWhiteSpace) || !int.TryParse(slug, out var tvdbId) || tvdbId <= 0)
                    {
                        return new List<Series>();
                    }

                    try
                    {
                        var existingSeries = _seriesService.FindByTvdbId(tvdbId);
                        if (existingSeries != null)
                        {
                            return new List<Series> { existingSeries };
                        }

                        return new List<Series> { GetSeriesInfo(tvdbId).Item1 };
                    }
                    catch (SeriesNotFoundException)
                    {
                        return new List<Series>();
                    }
                }

                var httpRequest = _requestBuilder.Create()
                                                 .SetSegment("route", "site")
                                                 .Resource("search")
                                                 .AddQueryParam("q", title.ToLower().Trim())
                                                 .Build();

                var httpResponse = _httpClient.Get<List<ShowResource>>(httpRequest);

                return httpResponse.Resource.SelectList(MapSearchResult);
            }
            catch (HttpException ex)
            {
                _logger.Warn(ex);
                throw new SkyHookException("Search for '{0}' failed. Unable to communicate with SkyHook.", ex, title);
            }
            catch (WebException ex)
            {
                _logger.Warn(ex);
                throw new SkyHookException("Search for '{0}' failed. Unable to communicate with SkyHook.", ex, title, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.Warn(ex);
                throw new SkyHookException("Search for '{0}' failed. Invalid response received from SkyHook.", ex, title);
            }
        }

        public List<Movie> SearchForNewMovies(string title)
        {
            try
            {
                var lowerTitle = title.ToLowerInvariant();

                if (lowerTitle.StartsWith("tmdb:") || lowerTitle.StartsWith("tmdbid:"))
                {
                    var slug = lowerTitle.Split(':')[1].Trim();

                    if (slug.IsNullOrWhiteSpace() || slug.Any(char.IsWhiteSpace) || !int.TryParse(slug, out var tmdbId) || tmdbId <= 0)
                    {
                        return new List<Movie>();
                    }

                    try
                    {
                        var existingMovie = _movieService.FindByTmdbId(tmdbId);
                        if (existingMovie != null)
                        {
                            return new List<Movie> { existingMovie };
                        }

                        return new List<Movie> { GetMovieInfo(tmdbId) };
                    }
                    catch (SeriesNotFoundException)
                    {
                        return new List<Movie>();
                    }
                }

                var httpRequest = _requestBuilder.Create()
                                                 .SetSegment("route", "movie")
                                                 .Resource("search")
                                                 .AddQueryParam("q", title.ToLower().Trim())
                                                 .Build();

                var httpResponse = _httpClient.Get<List<MovieResource>>(httpRequest);

                return httpResponse.Resource.SelectList(MapSearchResult);
            }
            catch (HttpException ex)
            {
                _logger.Warn(ex);
                throw new SkyHookException("Search for '{0}' failed. Unable to communicate with SkyHook.", ex, title);
            }
            catch (WebException ex)
            {
                _logger.Warn(ex);
                throw new SkyHookException("Search for '{0}' failed. Unable to communicate with SkyHook.", ex, title, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.Warn(ex);
                throw new SkyHookException("Search for '{0}' failed. Invalid response received from SkyHook.", ex, title);
            }
        }

        private Series MapSearchResult(ShowResource show)
        {
            var series = _seriesService.FindByTvdbId(show.ForeignId);

            if (series == null)
            {
                series = MapSeries(show);
            }

            return series;
        }

        private Movie MapSearchResult(MovieResource movieResource)
        {
            var movie = _movieService.FindByTmdbId(movieResource.ForeignId);

            if (movie == null)
            {
                movie = MapMovie(movieResource);
            }

            return movie;
        }

        private Series MapSeries(ShowResource show)
        {
            var series = new Series();
            series.TvdbId = show.ForeignId;

            series.Title = show.Title;
            series.CleanTitle = Parser.Parser.CleanSeriesTitle(show.Title);
            series.SortTitle = SeriesTitleNormalizer.Normalize(show.Title, show.ForeignId);

            series.OriginalLanguage = show.OriginalLanguage.IsNotNullOrWhiteSpace() ?
                IsoLanguages.Find(show.OriginalLanguage.ToLower())?.Language ?? Language.English :
                Language.English;

            if (show.FirstAired != null)
            {
                series.FirstAired = DateTime.ParseExact(show.FirstAired, "yyyy-MM-dd", DateTimeFormatInfo.InvariantInfo, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);
                series.Year = series.FirstAired.Value.Year;
            }

            series.Overview = show.Overview;

            if (show.Runtime != null)
            {
                series.Runtime = show.Runtime.Value;
            }

            series.Network = show.Network;

            series.TitleSlug = show.Slug;
            series.Status = MapSeriesStatus(show.Status);
            series.Ratings = MapRatings(show.Rating);
            series.Genres = show.Genres;

            if (show.ContentRating.IsNotNullOrWhiteSpace())
            {
                series.Certification = show.ContentRating.ToUpper();
            }

            var seasons = show.Episodes.Select(e => e.Year).Distinct().OrderBy(x => x);

            series.Year = seasons.FirstOrDefault();
            series.Seasons = seasons.Select(MapSeason).ToList();
            series.Images = show.Images.Select(MapImage).ToList();
            series.Monitored = true;

            return series;
        }

        private Movie MapMovie(MovieResource movieResource)
        {
            var movie = new Movie();

            movie.TmdbId = movieResource.ForeignId;
            movie.Title = movieResource.Title;
            movie.CleanTitle = Parser.Parser.CleanSeriesTitle(movieResource.Title);
            movie.SortTitle = SeriesTitleNormalizer.Normalize(movieResource.Title, movieResource.ForeignId);

            movie.Overview = movieResource.Overview;
            movie.Studio = movieResource.Studio;

            if (movieResource.Duration != null)
            {
                movie.Runtime = movieResource.Duration.Value;
            }

            movie.TitleSlug = movieResource.Slug;
            movie.Images = movieResource.Images.Select(MapImage).ToList();
            movie.Monitored = true;

            return movie;
        }

        private static Actor MapActors(ActorResource arg)
        {
            var newActor = new Actor
            {
                Name = arg.Name,
                Character = arg.Character,
                Gender = MapGender(arg.Gender),
                TpdbId = arg.ForeignId
            };

            newActor.Images = arg.Images.Select(MapImage).ToList();

            return newActor;
        }

        private static Gender MapGender(string gender)
        {
            if (gender == null)
            {
                return Gender.Other;
            }

            var lowerGender = gender.ToLowerInvariant();

            switch (lowerGender)
            {
                case "female":
                    return Gender.Female;
                case "male":
                    return Gender.Male;
                default:
                    return Gender.Other;
            }
        }

        private static Episode MapEpisode(EpisodeResource oracleEpisode)
        {
            var episode = new Episode();
            episode.TvdbId = oracleEpisode.ForeignId;
            episode.Overview = oracleEpisode.Overview;
            episode.SeasonNumber = oracleEpisode.Year;
            episode.AbsoluteEpisodeNumber = oracleEpisode.AbsoluteEpisodeNumber;
            episode.Title = oracleEpisode.Title;
            episode.Runtime = oracleEpisode.Duration.GetValueOrDefault();

            episode.AirDate = oracleEpisode.ReleaseDate;
            episode.AirDateUtc = DateTime.Parse(episode.AirDate, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.AdjustToUniversal);
            episode.Actors = oracleEpisode.Credits.Select(MapActors).ToList();

            episode.Ratings = new Ratings();

            // Don't include series fanart images as episode screenshot
            if (oracleEpisode.Image != null)
            {
                episode.Images.Add(new MediaCover.MediaCover(MediaCoverTypes.Screenshot, oracleEpisode.Image));
            }

            return episode;
        }

        private static Season MapSeason(int seasonResource)
        {
            return new Season
            {
                SeasonNumber = seasonResource,
                Images = new List<MediaCover.MediaCover>(),
                Monitored = true
            };
        }

        private static SeriesStatusType MapSeriesStatus(string status)
        {
            if (status.IsNotNullOrWhiteSpace() && status.Equals("ended", StringComparison.InvariantCultureIgnoreCase))
            {
                return SeriesStatusType.Ended;
            }

            return SeriesStatusType.Continuing;
        }

        private static Ratings MapRatings(RatingResource rating)
        {
            if (rating == null)
            {
                return new Ratings();
            }

            return new Ratings
            {
                Votes = rating.Count,
                Value = rating.Value
            };
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
                case "logo":
                    return MediaCoverTypes.Logo;
                case "poster":
                    return MediaCoverTypes.Poster;
                case "banner":
                    return MediaCoverTypes.Banner;
                case "fanart":
                    return MediaCoverTypes.Fanart;
                default:
                    return MediaCoverTypes.Unknown;
            }
        }
    }
}
