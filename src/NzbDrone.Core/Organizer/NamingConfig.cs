using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Organizer
{
    public class NamingConfig : ModelBase
    {
        public static NamingConfig Default => new NamingConfig
        {
            RenameEpisodes = false,
            ReplaceIllegalCharacters = true,
            ColonReplacementFormat = ColonReplacementFormat.Smart,
            CustomColonReplacementFormat = string.Empty,
            MultiEpisodeStyle = MultiEpisodeStyle.PrefixedRange,
            StandardEpisodeFormat = "{Site Title} - {Release-Date} - {Episode Title} [{Quality Full}]",
            JavEpisodeFormat = "{External Id}",
            SeriesFolderFormat = "{Site Title}"
        };

        public bool RenameEpisodes { get; set; }
        public bool ReplaceIllegalCharacters { get; set; }
        public ColonReplacementFormat ColonReplacementFormat { get; set; }
        public string CustomColonReplacementFormat { get; set; }
        public MultiEpisodeStyle MultiEpisodeStyle { get; set; }
        public string StandardEpisodeFormat { get; set; }
        public string JavEpisodeFormat { get; set; }
        public string SeriesFolderFormat { get; set; }
    }
}
