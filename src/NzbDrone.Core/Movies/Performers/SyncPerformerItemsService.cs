using System;
using System.Linq;
using NLog;
using NzbDrone.Core.Exceptions;
using NzbDrone.Core.ImportLists.ImportExclusions;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Core.Movies.Commands;

namespace NzbDrone.Core.Movies.Performers
{
    public class SyncPerformerItemsService : IExecute<SyncPerformerItemsCommand>
    {
        private readonly IProvideMovieInfo _movieInfo;
        private readonly IPerformerService _performerService;
        private readonly IMovieService _movieService;
        private readonly IAddMovieService _addMovieService;
        private readonly IImportExclusionsService _importExclusionService;

        private readonly Logger _logger;

        public SyncPerformerItemsService(IProvideMovieInfo movieInfo,
                                        IPerformerService performerService,
                                        IMovieService movieService,
                                        IAddMovieService addMovieService,
                                        IImportExclusionsService importExclusionsService,
                                        Logger logger)
        {
            _movieInfo = movieInfo;
            _performerService = performerService;
            _movieService = movieService;
            _addMovieService = addMovieService;
            _importExclusionService = importExclusionsService;
            _logger = logger;
        }

        private void SyncPerformerItems(Performer performer)
        {
            if (performer.Monitored)
            {
                var existingMovies = _movieService.AllMovieForeignIds();
                var performerScenes = _movieInfo.GetPerformerScenes(performer.ForeignId);
                var excludedMovies = _importExclusionService.GetAllExclusions().Select(e => e.ForeignId);
                var moviesToAdd = performerScenes.Where(m => !existingMovies.Contains(m)).Where(m => !excludedMovies.Contains(m));

                if (moviesToAdd.Any())
                {
                    _addMovieService.AddMovies(moviesToAdd.Select(m => new Movie
                    {
                        ForeignId = m,
                        QualityProfileId = performer.QualityProfileId,
                        RootFolderPath = performer.RootFolderPath,
                        AddOptions = new AddMovieOptions
                        {
                            SearchForMovie = performer.SearchOnAdd,
                            AddMethod = AddMovieMethod.Performer
                        },
                        Monitored = true,
                        Tags = performer.Tags
                    }).ToList(), true);
                }
            }
        }

        public void Execute(SyncPerformerItemsCommand message)
        {
            if (message.PerformerIds.Any())
            {
                foreach (var performerId in message.PerformerIds)
                {
                    var performer = _performerService.GetById(performerId);
                    SyncPerformerItems(performer);
                }
            }
            else
            {
                var allPerformers = _performerService.GetAllPerformers().OrderBy(c => c.SortName).ToList();

                foreach (var performer in allPerformers)
                {
                    try
                    {
                        SyncPerformerItems(performer);
                    }
                    catch (MovieNotFoundException)
                    {
                        _logger.Error("Performer '{0}' (StashDb {1}) was not found, it may have been removed from The Movie Database.", performer.Name, performer.ForeignId);
                    }
                }
            }
        }
    }
}
