using System;
using System.Linq;
using NLog;
using NzbDrone.Core.Exceptions;
using NzbDrone.Core.ImportLists.ImportExclusions;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Core.Movies.Commands;

namespace NzbDrone.Core.Movies.Studios
{
    public class SyncStudioItemsService : IExecute<SyncStudioItemsCommand>
    {
        private readonly IProvideMovieInfo _movieInfo;
        private readonly IStudioService _studioService;
        private readonly IMovieService _movieService;
        private readonly IAddMovieService _addMovieService;
        private readonly IImportExclusionsService _importExclusionService;

        private readonly Logger _logger;

        public SyncStudioItemsService(IProvideMovieInfo movieInfo,
                                        IStudioService studioService,
                                        IMovieService movieService,
                                        IAddMovieService addMovieService,
                                        IImportExclusionsService importExclusionsService,
                                        Logger logger)
        {
            _movieInfo = movieInfo;
            _studioService = studioService;
            _movieService = movieService;
            _addMovieService = addMovieService;
            _importExclusionService = importExclusionsService;
            _logger = logger;
        }

        private void SyncStudioItems(Studio studio)
        {
            if (studio.Monitored)
            {
                var existingMovies = _movieService.AllMovieForeignIds();
                var studioScenes = _movieInfo.GetStudioScenes(studio.ForeignId);
                var excludedMovies = _importExclusionService.GetAllExclusions().Select(e => e.ForeignId);
                var moviesToAdd = studioScenes.Where(m => !existingMovies.Contains(m)).Where(m => !excludedMovies.Contains(m));

                if (moviesToAdd.Any())
                {
                    _addMovieService.AddMovies(moviesToAdd.Select(m => new Movie
                    {
                        ForeignId = m,
                        QualityProfileId = studio.QualityProfileId,
                        RootFolderPath = studio.RootFolderPath,
                        AddOptions = new AddMovieOptions
                        {
                            SearchForMovie = studio.SearchOnAdd,
                            AddMethod = AddMovieMethod.Studio
                        },
                        Monitored = true,
                        Tags = studio.Tags
                    }).ToList(), true);
                }
            }
        }

        public void Execute(SyncStudioItemsCommand message)
        {
            if (message.StudioIds.Any())
            {
                foreach (var studioId in message.StudioIds)
                {
                    var studio = _studioService.GetById(studioId);
                    SyncStudioItems(studio);
                }
            }
            else
            {
                var allStudios = _studioService.GetAllStudios().OrderBy(c => c.SortTitle).ToList();

                foreach (var studio in allStudios)
                {
                    try
                    {
                        SyncStudioItems(studio);
                    }
                    catch (MovieNotFoundException)
                    {
                        _logger.Error("Studio '{0}' (StashDb {1}) was not found, it may have been removed from The Movie Database.", studio.Title, studio.ForeignId);
                    }
                }
            }
        }
    }
}
