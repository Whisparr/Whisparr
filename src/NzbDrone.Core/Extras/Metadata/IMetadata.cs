using System.Collections.Generic;
using NzbDrone.Core.Extras.Metadata.Files;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Movies;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.Extras.Metadata
{
    public interface IMetadata : IProvider
    {
        string GetFilenameAfterMove(Media movie, MediaFile movieFile, MetadataFile metadataFile);
        MetadataFile FindMetadataFile(Media movie, string path);
        MetadataFileResult MovieMetadata(Media movie, MediaFile movieFile);
        List<ImageFileResult> MovieImages(Media movie);
    }
}
