using NzbDrone.Core.Organizer;

namespace Whisparr.Api.V3.Config
{
    public class NamingExampleResource
    {
        public string SingleEpisodeExample { get; set; }
        public string SeriesFolderExample { get; set; }
        public string SeasonFolderExample { get; set; }
    }

    public static class NamingConfigResourceMapper
    {
        public static NamingConfigResource ToResource(this NamingConfig model)
        {
            return new NamingConfigResource
            {
                Id = model.Id,

                RenameEpisodes = model.RenameEpisodes,
                ReplaceIllegalCharacters = model.ReplaceIllegalCharacters,
                MultiEpisodeStyle = model.MultiEpisodeStyle,
                StandardEpisodeFormat = model.StandardEpisodeFormat,
                SeriesFolderFormat = model.SeriesFolderFormat,
                SeasonFolderFormat = model.SeasonFolderFormat

                // IncludeSeriesTitle
                // IncludeEpisodeTitle
                // IncludeQuality
                // ReplaceSpaces
                // Separator
                // NumberStyle
            };
        }

        public static void AddToResource(this BasicNamingConfig basicNamingConfig, NamingConfigResource resource)
        {
            resource.IncludeSeriesTitle = basicNamingConfig.IncludeSeriesTitle;
            resource.IncludeEpisodeTitle = basicNamingConfig.IncludeEpisodeTitle;
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

                RenameEpisodes = resource.RenameEpisodes,
                ReplaceIllegalCharacters = resource.ReplaceIllegalCharacters,
                MultiEpisodeStyle = resource.MultiEpisodeStyle,
                StandardEpisodeFormat = resource.StandardEpisodeFormat,
                SeriesFolderFormat = resource.SeriesFolderFormat,
                SeasonFolderFormat = resource.SeasonFolderFormat
            };
        }
    }
}
