using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Organizer
{
    public class NamingConfig : ModelBase
    {
        public static NamingConfig Default => new NamingConfig
        {
            RenameEpisodes = false,
            ReplaceIllegalCharacters = true,
            MultiEpisodeStyle = 0,
            StandardEpisodeFormat = "{Site Title} - S{season:00}E{episode:00} - {Episode Title} {Quality Full}",
            SeriesFolderFormat = "{Site Title}",
            SeasonFolderFormat = "Season {season}"
        };

        public bool RenameEpisodes { get; set; }
        public bool ReplaceIllegalCharacters { get; set; }
        public int MultiEpisodeStyle { get; set; }
        public string StandardEpisodeFormat { get; set; }
        public string SeriesFolderFormat { get; set; }
        public string SeasonFolderFormat { get; set; }
    }
}
