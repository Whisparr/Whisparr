using System.Collections.Generic;

namespace NzbDrone.Core.Movies
{
    public static class MovieTitleNormalizer
    {
        private static readonly Dictionary<string, string> PreComputedTitles = new Dictionary<string, string>
                                                                     {
                                                                         { "999999999", "a to z" },
                                                                     };

        public static string Normalize(string title, string tmdbid)
        {
            if (PreComputedTitles.ContainsKey(tmdbid))
            {
                return PreComputedTitles[tmdbid];
            }

            return Parser.Parser.NormalizeTitle(title).ToLower();
        }
    }
}
