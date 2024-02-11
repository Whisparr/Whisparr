using NzbDrone.Core.Organizer;
using Whisparr.Http.REST;

namespace Whisparr.Api.V3.Config
{
    public class NamingConfigResource : RestResource
    {
        public bool RenameMovies { get; set; }
        public bool RenameScenes { get; set; }
        public bool ReplaceIllegalCharacters { get; set; }
        public ColonReplacementFormat ColonReplacementFormat { get; set; }
        public string StandardMovieFormat { get; set; }
        public string MovieFolderFormat { get; set; }
        public string StandardSceneFormat { get; set; }
        public string SceneFolderFormat { get; set; }
    }
}
