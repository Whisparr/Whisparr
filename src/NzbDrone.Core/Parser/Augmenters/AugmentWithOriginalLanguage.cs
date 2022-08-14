using System;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Parser.Augmenters
{
    public class AugmentWithOriginalLanguage : IAugmentParsedMovieInfo
    {
        public Type HelperType
        {
            get
            {
                return typeof(Media);
            }
        }

        public ParsedMovieInfo AugmentMovieInfo(ParsedMovieInfo movieInfo, object helper)
        {
            if (helper is Media movie && movie?.MediaMetadata.Value.OriginalLanguage != null && movieInfo != null)
            {
                movieInfo.ExtraInfo["OriginalLanguage"] = movie.MediaMetadata.Value.OriginalLanguage;
            }

            return movieInfo;
        }
    }
}
