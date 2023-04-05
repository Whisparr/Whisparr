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
            MultiEpisodeStyle = MultiEpisodeStyle.PrefixedRange,
            StandardEpisodeFormat = "{Site Title} - {Release-Date} - {Episode Title} [{Quality Full}]",
            StandardMovieFormat = "{Movie Title} ({Movie Year}) {Quality Full}",
            SeriesFolderFormat = "{Site Title}",
            MovieFolderFormat = "{Movie Title} ({Movie Year})"
        };

        public bool RenameEpisodes { get; set; }
        public bool ReplaceIllegalCharacters { get; set; }
        public ColonReplacementFormat ColonReplacementFormat { get; set; }
        public MultiEpisodeStyle MultiEpisodeStyle { get; set; }
        public string StandardEpisodeFormat { get; set; }
        public string StandardMovieFormat { get; set; }
        public string SeriesFolderFormat { get; set; }
        public string MovieFolderFormat { get; set; }
    }
}
