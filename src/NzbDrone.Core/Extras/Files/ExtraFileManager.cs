using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Movies;

namespace NzbDrone.Core.Extras.Files
{
    public interface IManageExtraFiles
    {
        int Order { get; }
        IEnumerable<ExtraFile> CreateAfterMediaCoverUpdate(Media movie);
        IEnumerable<ExtraFile> CreateAfterMovieScan(Media movie, List<MediaFile> movieFiles);
        IEnumerable<ExtraFile> CreateAfterMovieImport(Media movie, MediaFile movieFile);
        IEnumerable<ExtraFile> CreateAfterMovieFolder(Media movie, string movieFolder);
        IEnumerable<ExtraFile> MoveFilesAfterRename(Media movie, List<MediaFile> movieFiles);
        ExtraFile Import(Media movie, MediaFile movieFile, string path, string extension, bool readOnly);
    }

    public abstract class ExtraFileManager<TExtraFile> : IManageExtraFiles
        where TExtraFile : ExtraFile, new()
    {
        private readonly IConfigService _configService;
        private readonly IDiskProvider _diskProvider;
        private readonly IDiskTransferService _diskTransferService;
        private readonly Logger _logger;

        public ExtraFileManager(IConfigService configService,
                                IDiskProvider diskProvider,
                                IDiskTransferService diskTransferService,
                                Logger logger)
        {
            _configService = configService;
            _diskProvider = diskProvider;
            _diskTransferService = diskTransferService;
            _logger = logger;
        }

        public abstract int Order { get; }
        public abstract IEnumerable<ExtraFile> CreateAfterMediaCoverUpdate(Media movie);
        public abstract IEnumerable<ExtraFile> CreateAfterMovieScan(Media movie, List<MediaFile> movieFiles);
        public abstract IEnumerable<ExtraFile> CreateAfterMovieImport(Media movie, MediaFile movieFile);
        public abstract IEnumerable<ExtraFile> CreateAfterMovieFolder(Media movie, string movieFolder);
        public abstract IEnumerable<ExtraFile> MoveFilesAfterRename(Media movie, List<MediaFile> movieFiles);
        public abstract ExtraFile Import(Media movie, MediaFile movieFile, string path, string extension, bool readOnly);

        protected TExtraFile ImportFile(Media movie, MediaFile movieFile, string path, bool readOnly, string extension, string fileNameSuffix = null)
        {
            var movieFilePath = Path.Combine(movie.Path, movieFile.RelativePath);
            var newFolder = Path.GetDirectoryName(movieFilePath);
            var filenameBuilder = new StringBuilder(Path.GetFileNameWithoutExtension(movieFile.RelativePath));

            if (fileNameSuffix.IsNotNullOrWhiteSpace())
            {
                filenameBuilder.Append(fileNameSuffix);
            }

            filenameBuilder.Append(extension);

            var newFileName = Path.Combine(newFolder, filenameBuilder.ToString());

            if (newFileName == movieFilePath)
            {
                _logger.Debug("Extra file {0} not imported, due to naming interference with movie file", path);
                return null;
            }

            var transferMode = TransferMode.Move;

            if (readOnly)
            {
                transferMode = _configService.CopyUsingHardlinks ? TransferMode.HardLinkOrCopy : TransferMode.Copy;
            }

            _diskTransferService.TransferFile(path, newFileName, transferMode, true);

            return new TExtraFile
            {
                MovieId = movie.Id,
                MovieFileId = movieFile.Id,
                RelativePath = movie.Path.GetRelativePath(newFileName),
                Extension = extension
            };
        }

        protected TExtraFile MoveFile(Media movie, MediaFile movieFile, TExtraFile extraFile, string fileNameSuffix = null)
        {
            var newFolder = Path.GetDirectoryName(Path.Combine(movie.Path, movieFile.RelativePath));
            var filenameBuilder = new StringBuilder(Path.GetFileNameWithoutExtension(movieFile.RelativePath));

            if (fileNameSuffix.IsNotNullOrWhiteSpace())
            {
                filenameBuilder.Append(fileNameSuffix);
            }

            filenameBuilder.Append(extraFile.Extension);

            var existingFileName = Path.Combine(movie.Path, extraFile.RelativePath);
            var newFileName = Path.Combine(newFolder, filenameBuilder.ToString());

            if (newFileName.PathNotEquals(existingFileName))
            {
                try
                {
                    _diskProvider.MoveFile(existingFileName, newFileName);
                    extraFile.RelativePath = movie.Path.GetRelativePath(newFileName);

                    return extraFile;
                }
                catch (Exception ex)
                {
                    _logger.Warn(ex, "Unable to move file after rename: {0}", existingFileName);
                }
            }

            return null;
        }
    }
}
