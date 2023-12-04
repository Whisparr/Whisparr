using NzbDrone.Core.Organizer;

namespace Whisparr.Api.V3.Config
{
    public class NamingExampleResource
    {
        public string MovieExample { get; set; }
        public string MovieFolderExample { get; set; }
        public string SceneExample { get; set; }
        public string SceneFolderExample { get; set; }
    }

    public static class NamingConfigResourceMapper
    {
        public static NamingConfigResource ToResource(this NamingConfig model)
        {
            return new NamingConfigResource
            {
                Id = model.Id,

                RenameMovies = model.RenameMovies,
                RenameScenes = model.RenameScenes,
                ReplaceIllegalCharacters = model.ReplaceIllegalCharacters,
                ColonReplacementFormat = model.ColonReplacementFormat,
                StandardMovieFormat = model.StandardMovieFormat,
                MovieFolderFormat = model.MovieFolderFormat,
                StandardSceneFormat = model.StandardSceneFormat,
                SceneFolderFormat = model.SceneFolderFormat,

                // IncludeQuality
                // ReplaceSpaces
                // Separator
                // NumberStyle
            };
        }

        public static void AddToResource(this BasicNamingConfig basicNamingConfig, NamingConfigResource resource)
        {
            resource.IncludeQuality = basicNamingConfig.IncludeQuality;
            resource.ReplaceSpaces = basicNamingConfig.ReplaceSpaces;
            resource.Separator = basicNamingConfig.Separator;
            resource.NumberStyle = basicNamingConfig.NumberStyle;
        }

        public static NamingConfig ToModel(this NamingConfigResource resource)
        {
            return new NamingConfig
            {
                Id = resource.Id,

                RenameMovies = resource.RenameMovies,
                RenameScenes = resource.RenameScenes,
                ReplaceIllegalCharacters = resource.ReplaceIllegalCharacters,
                ColonReplacementFormat = resource.ColonReplacementFormat,
                StandardMovieFormat = resource.StandardMovieFormat,
                MovieFolderFormat = resource.MovieFolderFormat,
                StandardSceneFormat = resource.StandardSceneFormat,
                SceneFolderFormat = resource.SceneFolderFormat,
            };
        }
    }
}
