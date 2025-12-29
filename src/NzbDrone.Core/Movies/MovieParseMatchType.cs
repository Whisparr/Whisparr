namespace NzbDrone.Core.Movies
{
    public enum MovieParseMatchType
    {
        StashId = 1,
        Title = 2,
        Episode = 3,
        PerformersTitle = 4,
        CharactersTitle = 5,
        Performers = 6,
        Characters = 7,
        PerformerTitle = 8,
        CharacterTitle = 9,
        PerformersNotTitle = 10,
        CharactersNotTitle = 11,
        ParsedTitleContainsCleanTitle = 12
    }
}
