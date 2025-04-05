using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.MediaFiles.MovieImport.Aggregation;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.RootFolders;

namespace NzbDrone.Core.MediaFiles.MovieImport
{
    public interface ISceneIdentificationService
    {
        Movie Identify(string file, ParsedMovieInfo parsedMovieInfo);
    }

    public class SceneIdentificationService : ISceneIdentificationService
    {
        private readonly ISearchForNewMovie _searchProxy;
        private readonly IAddMovieService _addMovieService;
        private readonly IAggregationService _aggregationService;
        private readonly IDiskProvider _diskProvider;
        private readonly IDiskTransferService _diskTransferService;
        private readonly IBuildFileNames _fileNameBuilder;
        private readonly IMapCoversToLocal _coverMapper;
        private readonly IMovieService _movieService;
        private readonly IMovieMetadataService _movieMetadataService;
        private readonly INamingConfigService _namingConfigService;
        private readonly IConfigService _configService;
        private readonly IRootFolderService _rootFolderService;
        private readonly Logger _logger;

        public SceneIdentificationService(ISearchForNewMovie searchProxy,
                                          IAddMovieService addMovieService,
                                          IAggregationService aggregationService,
                                          IDiskProvider diskProvider,
                                          IDiskTransferService diskTransferService,
                                          IBuildFileNames fileNameBuilder,
                                          IMapCoversToLocal coverMapper,
                                          IMovieService movieService,
                                          IMovieMetadataService movieMetadataService,
                                          INamingConfigService namingConfigService,
                                          IConfigService configService,
                                          IRootFolderService rootFolderService,
                                          Logger logger)
        {
            _searchProxy = searchProxy;
            _addMovieService = addMovieService;
            _aggregationService = aggregationService;
            _diskProvider = diskProvider;
            _diskTransferService = diskTransferService;
            _fileNameBuilder = fileNameBuilder;
            _coverMapper = coverMapper;
            _movieService = movieService;
            _movieMetadataService = movieMetadataService;
            _namingConfigService = namingConfigService;
            _configService = configService;
            _rootFolderService = rootFolderService;
            _logger = logger;
        }

        public Movie Identify(string file, ParsedMovieInfo parsedMovieInfo)
        {
            var namingConfig = _namingConfigService.GetConfig();
            var rootFolder = _rootFolderService.GetBestRootFolderPath(file);

            var pattern = namingConfig.SceneFolderFormat;
            var sceneFolderName = pattern.Split(new char[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries)[0];

            var scenesFolder = Path.Combine(rootFolder, sceneFolderName);
            var fileName = Path.GetFileName(file);

            var studioTitleSlug = _fileNameBuilder.CleanTitle(parsedMovieInfo.StudioTitle);
            var releaseDate = parsedMovieInfo.ReleaseDate;
            var firstPerformer = parsedMovieInfo.FirstPerformer;
            string term;

            var searchResults = new List<Movie>();
            var searchedByStashId = false;

            // Try to see if the scene has been organized into a folder already
            var folderRegex = new Regex(@"(?<airyear>\d{2}|\d{4})[-_. ]+(?<airmonth>[0-1][0-9])[-_. ]+(?<airday>[0-3][0-9])",
                RegexOptions.IgnoreCase | RegexOptions.Compiled);
            var folder = Directory.GetParent(file).Name;

            var match = folderRegex.Match(folder);
            if (match.Success)
            {
                var sceneSearch = folder.Replace(" - ", " ");
                searchResults = _searchProxy.SearchForNewScene(sceneSearch);
                releaseDate = match.Groups[0].ToString();
            }
            else if (parsedMovieInfo.StashId.IsNotNullOrWhiteSpace())
            {
                // Search by StashId
                searchResults = _searchProxy.SearchForNewScene(parsedMovieInfo.StashId);
                searchedByStashId = true;
            }
            else
            {
                term = $"{studioTitleSlug} {releaseDate} {firstPerformer}";
                searchResults = _searchProxy.SearchForNewScene(term);
            }

            // Get the best match for the movie
            var result = new Movie();

            var tempTitle = string.Join(" ", parsedMovieInfo.ReleaseTokens.Split("."));
            var parsedMovieTitle = Parser.Parser.NormalizeEpisodeTitle(tempTitle);
            if (searchedByStashId && searchResults.Count == 1)
            {
                // If we searched by StashId and found results, use the first result directly
                result = searchResults.First();
            }
            else if (parsedMovieTitle != null && !searchedByStashId)
            {
                var matches = _movieService.MatchMovies(parsedMovieTitle, releaseDate, searchResults);

                if (matches.Count == 1)
                {
                    result = matches.First().Key;
                }
            }

            var movie = new Movie();

            if (result.Title != null)
            {
                movie = result;
                var sourcePath = file.ToString();
                movie.Path = sourcePath;

                var movieFolder = _fileNameBuilder.GetMovieFolder(movie, namingConfig);
                var destinationFolder = Path.Combine(rootFolder, movieFolder);

                // Build the new filename to avoid renaming in later
                // if renaming isn't activated, the file will be moved to the new folder
                var localMovie = new LocalMovie
                {
                    Movie = movie,
                    FolderMovieInfo = parsedMovieInfo,
                    FileMovieInfo = parsedMovieInfo,
                    Path = movie.Path,
                };

                var movieFile = new MovieFile
                {
                    Path = fileName,
                    OriginalFilePath = fileName,
                    Movie = movie,
                };

                localMovie = _aggregationService.Augment(localMovie, null);
                movieFile.Quality = localMovie.Quality;

                var movieExists = new Movie();
                if (_movieMetadataService.FindByForeignId(result.MovieMetadata.Value.ForeignId) != null)
                {
                    movieExists = _movieService.FindByForeignId(result.MovieMetadata.Value.ForeignId);
                    if (movieExists != null)
                    {
                        movie = movieExists;
                    }
                }

                var newName = _fileNameBuilder.BuildFileName(movie, movieFile);
                var newFileName = newName + Path.GetExtension(fileName);

                if (!_diskProvider.FolderExists(destinationFolder))
                {
                    _diskProvider.CreateFolder(destinationFolder);
                }

                var destinationPath = Path.Combine(rootFolder, destinationFolder, newFileName);
                if (sourcePath != destinationPath)
                {
                    try
                    {
                        _diskTransferService.TransferFile(sourcePath, destinationPath, TransferMode.Move);
                    }
                    catch
                    {
                        _logger.Info("Possible duplicate file - {0}", sourcePath);
                        movie.Path = destinationPath;
                        return movie;
                    }
                }

                // Check if the ForeignId exists already
                if (_movieMetadataService.FindByForeignId(result.MovieMetadata.Value.ForeignId) != null)
                {
                    if (movieExists != null)
                    {
                        movie.Path = destinationPath;
                        return movie;
                    }
                }

                movie.QualityProfileId = 1;
                movie.RootFolderPath = rootFolder;
                movie.AddOptions = new AddMovieOptions
                {
                    SearchForMovie = false,
                    AddMethod = AddMovieMethod.Manual
                };
                movie.Monitored = true;
                movie.Path = destinationFolder;
            }

            return movie;
        }
    }
}
