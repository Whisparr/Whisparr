using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Organizer
{
    public class NamingConfig : ModelBase
    {
        public static NamingConfig Default => new NamingConfig
        {
            RenameMovies = false,
            RenameScenes = false,
            ReplaceIllegalCharacters = true,
            ColonReplacementFormat = ColonReplacementFormat.Smart,
            MovieFolderFormat = "movies/{Movie Title} ({Release Year})",
            SceneFolderFormat = "scenes/{Studio Title}/{Scene Title} - {Release Date}",
            SceneImportFolderFormat = "import/",
            StandardMovieFormat = "{Movie Title} ({Release Year}) {Quality Full}",
            StandardSceneFormat = "{Scene Title} - {Release Date} {Quality Full}"
        };

        public bool RenameMovies { get; set; }
        public bool RenameScenes { get; set; }
        public bool ReplaceIllegalCharacters { get; set; }
        public ColonReplacementFormat ColonReplacementFormat { get; set; }
        public string StandardMovieFormat { get; set; }
        public string StandardSceneFormat { get; set; }
        public string MovieFolderFormat { get; set; }
        public string SceneFolderFormat { get; set; }
        public string SceneImportFolderFormat { get; set; }
    }
}
