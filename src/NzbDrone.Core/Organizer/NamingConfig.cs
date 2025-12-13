using NzbDrone.Common.EnvironmentInfo;
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
            MovieFolderFormat = OsInfo.IsWindows ? "movies\\{Movie Title} ({Release Year})" :  "movies/{Movie Title} ({Release Year})",
            SceneFolderFormat = OsInfo.IsWindows ? "scenes\\{Studio Title}\\{Scene Title} - {Release Date}" : "scenes/{Studio Title}/{Scene Title} - {Release Date}",
            SceneImportFolderFormat = OsInfo.IsWindows ? "import\\" : "import/",
            StandardMovieFormat = "{Movie Title} ({Release Year}) {Quality Full}",
            StandardSceneFormat = "{Scene Title} - {Release Date} {Quality Full}",
            MaxFolderPathLength = OsInfo.IsWindows ? 240 : 255,
            MaxFilePathLength = OsInfo.IsWindows ? 248 : 255,
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
        public int MaxFolderPathLength { get; set; }
        public int MaxFilePathLength { get; set; }
    }
}
