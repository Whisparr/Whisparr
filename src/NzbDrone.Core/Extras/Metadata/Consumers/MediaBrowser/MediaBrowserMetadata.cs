using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Extras.Metadata.Files;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Movies;

namespace NzbDrone.Core.Extras.Metadata.Consumers.MediaBrowser
{
    public class MediaBrowserMetadata : MetadataBase<MediaBrowserMetadataSettings>
    {
        private readonly Logger _logger;

        public MediaBrowserMetadata(
                            Logger logger)
        {
            _logger = logger;
        }

        public override string Name => "Emby (Legacy)";

        public override MetadataFile FindMetadataFile(Media movie, string path)
        {
            var filename = Path.GetFileName(path);

            if (filename == null)
            {
                return null;
            }

            var metadata = new MetadataFile
            {
                MovieId = movie.Id,
                Consumer = GetType().Name,
                RelativePath = movie.Path.GetRelativePath(path)
            };

            if (filename.Equals("movie.xml", StringComparison.InvariantCultureIgnoreCase))
            {
                metadata.Type = MetadataType.MovieMetadata;
                return metadata;
            }

            return null;
        }

        public override MetadataFileResult MovieMetadata(Media movie, MediaFile movieFile)
        {
            if (!Settings.MovieMetadata)
            {
                return null;
            }

            _logger.Debug("Generating movie.xml for: {0}", movie.Title);
            var sb = new StringBuilder();
            var xws = new XmlWriterSettings();
            xws.OmitXmlDeclaration = true;
            xws.Indent = false;

            using (var xw = XmlWriter.Create(sb, xws))
            {
                var movieElement = new XElement("Movie");

                movieElement.Add(new XElement("id", movie.MediaMetadata.Value.ForiegnId));
                movieElement.Add(new XElement("Status", movie.MediaMetadata.Value.Status));

                movieElement.Add(new XElement("Added", movie.Added.ToString("MM/dd/yyyy HH:mm:ss tt")));
                movieElement.Add(new XElement("LockData", "false"));
                movieElement.Add(new XElement("Overview", movie.MediaMetadata.Value.Overview));
                movieElement.Add(new XElement("LocalTitle", movie.Title));

                movieElement.Add(new XElement("Rating", movie.MediaMetadata.Value.Ratings.Tmdb?.Value ?? 0));
                movieElement.Add(new XElement("ProductionYear", movie.Year));
                movieElement.Add(new XElement("RunningTime", movie.MediaMetadata.Value.Runtime));
                movieElement.Add(new XElement("Genres", movie.MediaMetadata.Value.Genres.Select(genre => new XElement("Genre", genre))));

                var doc = new XDocument(movieElement);
                doc.Save(xw);

                _logger.Debug("Saving movie.xml for {0}", movie.Title);

                return new MetadataFileResult("movie.xml", doc.ToString());
            }
        }

        public override List<ImageFileResult> MovieImages(Media movie)
        {
            return new List<ImageFileResult>();
        }

        private IEnumerable<ImageFileResult> ProcessMovieImages(Media movie)
        {
            return new List<ImageFileResult>();
        }
    }
}
