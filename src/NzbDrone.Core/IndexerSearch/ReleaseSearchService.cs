using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Instrumentation.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Movies.Studios;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Profiles.Qualities;

namespace NzbDrone.Core.IndexerSearch
{
    public interface ISearchForReleases
    {
        Task<List<DownloadDecision>> MovieSearch(int movieId, bool userInvokedSearch, bool interactiveSearch);
        Task<List<DownloadDecision>> MovieSearch(Movie movie, bool userInvokedSearch, bool interactiveSearch);
    }

    public class ReleaseSearchService : ISearchForReleases
    {
        private readonly IIndexerFactory _indexerFactory;
        private readonly IMakeDownloadDecision _makeDownloadDecision;
        private readonly IMovieService _movieService;
        private readonly IStudioService _studioService;
        private readonly IQualityProfileService _qualityProfileService;
        private readonly IConfigService _configService;
        private readonly Logger _logger;

        public ReleaseSearchService(IIndexerFactory indexerFactory,
                                IMakeDownloadDecision makeDownloadDecision,
                                IMovieService movieService,
                                IStudioService studioService,
                                IQualityProfileService qualityProfileService,
                                IConfigService configService,
                                Logger logger)
        {
            _indexerFactory = indexerFactory;
            _makeDownloadDecision = makeDownloadDecision;
            _movieService = movieService;
            _studioService = studioService;
            _qualityProfileService = qualityProfileService;
            _configService = configService;
            _logger = logger;
        }

        public async Task<List<DownloadDecision>> MovieSearch(int movieId, bool userInvokedSearch, bool interactiveSearch)
        {
            var movie = _movieService.GetMovie(movieId);

            return await MovieSearch(movie, userInvokedSearch, interactiveSearch);
        }

        public async Task<List<DownloadDecision>> MovieSearch(Movie movie, bool userInvokedSearch, bool interactiveSearch)
        {
            var downloadDecisions = new List<DownloadDecision>();

            var decisions = new List<DownloadDecision>();

            if (movie.MovieMetadata.Value.ItemType == ItemType.Movie)
            {
                var movieSearchSpec = Get<MovieSearchCriteria>(movie, userInvokedSearch, interactiveSearch);

                // For movies, add search with year
                if (movie.Year > 1900)
                {
                    movieSearchSpec.SceneTitles.AddRange(movieSearchSpec.SceneTitles.Select(title => $"{title} {movie.Year}").ToList());
                }

                decisions = await Dispatch(indexer => indexer.Fetch(movieSearchSpec), movieSearchSpec);
            }
            else
            {
                var sceneSearchSpec = Get<SceneSearchCriteria>(movie, userInvokedSearch, interactiveSearch);
                sceneSearchSpec.ReleaseDate = DateOnly.FromDateTime(movie.MovieMetadata.Value.ReleaseDateUtc.Value);
                sceneSearchSpec.SiteTitle = movie.MovieMetadata.Value.StudioTitle;

                var releaseDateStrings = new List<string>();

                // When we search a studio name, we inject the date at the search service level.
                // This was previously done at indexer level, however this denies us an opportunity to issue queries which may not include the date,
                // but could include other identifying information to provide more options for the release to be found by the matcher.
                if ((sceneSearchSpec.ReleaseDate?.ToString("yy.MM.dd") ?? string.Empty).IsNotNullOrWhiteSpace())
                {
                    if (_configService.SearchDateFormat == SearchDateFormatType.YYMMDD || _configService.SearchDateFormat == SearchDateFormatType.BOTH)
                    {
                        releaseDateStrings.Add(sceneSearchSpec.ReleaseDate?.ToString("yy.MM.dd") ?? string.Empty);
                    }

                    if (_configService.SearchDateFormat == SearchDateFormatType.DDMMYYYY || _configService.SearchDateFormat == SearchDateFormatType.BOTH)
                    {
                        releaseDateStrings.Add(sceneSearchSpec.ReleaseDate?.ToString("dd.MM.yyyy") ?? string.Empty);
                    }
                }

                // The sceneSearchSpec.SceneTitles list contains MovieMetadata.Title, we will inject the date here vs in the indexers.
                var originalTitles = sceneSearchSpec.SceneTitles;
                sceneSearchSpec.SceneTitles = new List<string>();

                // Search for Scene Name
                if (_configService.SearchTitleOnly)
                {
                    sceneSearchSpec.SceneTitles.Add(sceneSearchSpec.Movie.Title);
                }

                if (_configService.SearchTitleDate)
                {
                    foreach (var releaseDateString in releaseDateStrings)
                    {
                        // Search for Scene Name + Date
                        sceneSearchSpec.SceneTitles.AddRange(originalTitles.Select(title => $"{title} {releaseDateString}").ToList());
                    }
                }

                if (sceneSearchSpec.SiteTitle != null)
                {
                    var studioTitles = _studioService.FindAllByTitle(sceneSearchSpec.SiteTitle);
                    foreach (var studioTitle in studioTitles)
                    {
                        if (_configService.SearchStudioFormat == SearchStudioFormatType.ORIGINAL || _configService.SearchStudioFormat == SearchStudioFormatType.BOTH)
                        {
                            // Full Studio Name (Couch Casting-X)
                            sceneSearchSpec.SceneTitles = generateSceneTitles(sceneSearchSpec.SceneTitles, studioTitle.Title, releaseDateStrings, originalTitles);
                        }

                        if (_configService.SearchStudioFormat == SearchStudioFormatType.CLEAN || _configService.SearchStudioFormat == SearchStudioFormatType.BOTH)
                        {
                            // Studio with spaces and other characters removed (CouchCastingX)
                            sceneSearchSpec.SceneTitles = generateSceneTitles(sceneSearchSpec.SceneTitles, studioTitle.CleanTitle, releaseDateStrings, originalTitles);
                        }
                    }
                }

                decisions = await Dispatch(indexer => indexer.Fetch(sceneSearchSpec), sceneSearchSpec);
            }

            downloadDecisions.AddRange(decisions);

            return DeDupeDecisions(downloadDecisions);
        }

        private List<string> generateSceneTitles(List<string> sceneTitles, string studioTitle, List<string> releaseDateStrings, List<string> originalTitles)
        {
            if (_configService.SearchStudioTitle)
            {
                // Search for Studio + Scene Name
                foreach (var originalTitle in originalTitles)
                {
                    // Search for Studio + Date
                    if (!sceneTitles.Contains($"{studioTitle} {originalTitle}"))
                    {
                        sceneTitles.Add($"{studioTitle} {originalTitle}");
                    }
                }
            }

            if (_configService.SearchStudioDate)
            {
                // Search for Studio + Date
                foreach (var releaseDateString in releaseDateStrings)
                {
                    if (!sceneTitles.Contains($"{studioTitle} {releaseDateString}"))
                    {
                        sceneTitles.Add($"{studioTitle} {releaseDateString}");
                    }
                }
            }

            return sceneTitles;
        }

        private TSpec Get<TSpec>(Movie movie, bool userInvokedSearch, bool interactiveSearch)
            where TSpec : SearchCriteriaBase, new()
        {
            var spec = new TSpec
            {
                Movie = movie,
                UserInvokedSearch = userInvokedSearch,
                InteractiveSearch = interactiveSearch
            };

            var wantedLanguages = _qualityProfileService.GetAcceptableLanguages(movie.QualityProfileId);

            var queryTranslations = new List<string>
            {
                movie.MovieMetadata.Value.Title
            };

            spec.SceneTitles = queryTranslations.Distinct().Where(t => t.IsNotNullOrWhiteSpace()).ToList();

            return spec;
        }

        private async Task<List<DownloadDecision>> Dispatch(Func<IIndexer, Task<IList<ReleaseInfo>>> searchAction, SearchCriteriaBase criteriaBase)
        {
            var indexers = criteriaBase.InteractiveSearch ?
                _indexerFactory.InteractiveSearchEnabled() :
                _indexerFactory.AutomaticSearchEnabled();

            // Filter indexers to untagged indexers and indexers with intersecting tags
            indexers = indexers.Where(i => i.Definition.Tags.Empty() || i.Definition.Tags.Intersect(criteriaBase.Movie.Tags).Any()).ToList();

            _logger.ProgressInfo("Searching indexers for {0}. {1} active indexers", criteriaBase, indexers.Count);

            var tasks = indexers.Select(indexer => DispatchIndexer(searchAction, indexer, criteriaBase));

            var batch = await Task.WhenAll(tasks);

            var reports = batch.SelectMany(x => x).ToList();

            _logger.Debug("Total of {0} reports were found for {1} from {2} indexers", reports.Count, criteriaBase, indexers.Count);

            // Update the last search time for movie if at least 1 indexer was searched.
            if (indexers.Any())
            {
                var lastSearchTime = DateTime.UtcNow;
                _logger.Debug("Setting last search time to: {0}", lastSearchTime);

                criteriaBase.Movie.LastSearchTime = lastSearchTime;
                _movieService.UpdateLastSearchTime(criteriaBase.Movie);
            }

            return _makeDownloadDecision.GetSearchDecision(reports, criteriaBase).ToList();
        }

        private async Task<IList<ReleaseInfo>> DispatchIndexer(Func<IIndexer, Task<IList<ReleaseInfo>>> searchAction, IIndexer indexer, SearchCriteriaBase criteriaBase)
        {
            try
            {
                return await searchAction(indexer);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error while searching for {0}", criteriaBase);
            }

            return Array.Empty<ReleaseInfo>();
        }

        private List<DownloadDecision> DeDupeDecisions(List<DownloadDecision> decisions)
        {
            // De-dupe reports by guid so duplicate results aren't returned. Pick the one with the least rejections and higher indexer priority.
            return decisions.GroupBy(d => d.RemoteMovie.Release.Guid)
                .Select(d => d.OrderBy(v => v.Rejections.Count()).ThenBy(v => v.RemoteMovie?.Release?.IndexerPriority ?? IndexerDefinition.DefaultPriority).First())
                .ToList();
        }
    }
}
