using System;
using System.Collections.Generic;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.MediaFiles
{
    public static class MediaFileExtensions
    {
        private static Dictionary<string, Quality> _fileExtensions;

        static MediaFileExtensions()
        {
            _fileExtensions = new Dictionary<string, Quality>(StringComparer.OrdinalIgnoreCase)
            {
                // Unknown
                { ".webm", Quality.Unknown },

                // WEBDL480p
                { ".m4v", Quality.WEBDL480p },
                { ".3gp", Quality.WEBDL480p },
                { ".nsv", Quality.WEBDL480p },
                { ".ty", Quality.WEBDL480p },
                { ".strm", Quality.WEBDL480p },
                { ".rm", Quality.WEBDL480p },
                { ".rmvb", Quality.WEBDL480p },
                { ".m3u", Quality.WEBDL480p },
                { ".ifo", Quality.WEBDL480p },
                { ".mov", Quality.WEBDL480p },
                { ".qt", Quality.WEBDL480p },
                { ".divx", Quality.WEBDL480p },
                { ".xvid", Quality.WEBDL480p },
                { ".bivx", Quality.WEBDL480p },
                { ".nrg", Quality.WEBDL480p },
                { ".pva", Quality.WEBDL480p },
                { ".wmv", Quality.WEBDL480p },
                { ".asf", Quality.WEBDL480p },
                { ".asx", Quality.WEBDL480p },
                { ".ogm", Quality.WEBDL480p },
                { ".ogv", Quality.WEBDL480p },
                { ".m2v", Quality.WEBDL480p },
                { ".avi", Quality.WEBDL480p },
                { ".bin", Quality.WEBDL480p },
                { ".dat", Quality.WEBDL480p },
                { ".dvr-ms", Quality.WEBDL480p },
                { ".mpg", Quality.WEBDL480p },
                { ".mpeg", Quality.WEBDL480p },
                { ".mp4", Quality.WEBDL480p },
                { ".avc", Quality.WEBDL480p },
                { ".vp3", Quality.WEBDL480p },
                { ".svq3", Quality.WEBDL480p },
                { ".nuv", Quality.WEBDL480p },
                { ".viv", Quality.WEBDL480p },
                { ".dv", Quality.WEBDL480p },
                { ".fli", Quality.WEBDL480p },
                { ".flv", Quality.WEBDL480p },
                { ".wpl", Quality.WEBDL480p },
                { ".f4v", Quality.WEBDL480p },

                // DVD
                { ".img", Quality.DVD },
                { ".iso", Quality.DVD },
                { ".vob", Quality.DVD },

                // HD
                { ".mkv", Quality.WEBDL720p },
                { ".ts", Quality.WEBDL720p },
                { ".wtv", Quality.WEBDL720p },

                // Bluray
                { ".m2ts", Quality.Bluray720p }
            };
        }

        public static HashSet<string> Extensions => new HashSet<string>(_fileExtensions.Keys, StringComparer.OrdinalIgnoreCase);
        public static HashSet<string> DiskExtensions => new HashSet<string>(new[] { ".img", ".iso", ".vob" }, StringComparer.OrdinalIgnoreCase);

        public static Quality GetQualityForExtension(string extension)
        {
            if (_fileExtensions.ContainsKey(extension))
            {
                return _fileExtensions[extension];
            }

            return Quality.Unknown;
        }
    }
}
