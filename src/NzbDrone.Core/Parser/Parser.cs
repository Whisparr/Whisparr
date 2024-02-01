using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Instrumentation;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Parser
{
    public static class Parser
    {
        private static readonly Logger Logger = NzbDroneLogger.GetLogger(typeof(Parser));

        private static readonly Regex EditionRegex = new Regex(@"\(?\b(?<edition>(((Recut.|Extended.|Ultimate.)?(Director.?s|Collector.?s|Theatrical|Ultimate|Extended|Despecialized|(Special|Rouge|Final|Assembly|Imperial|Diamond|Signature|Hunter|Rekall)(?=(.(Cut|Edition|Version)))|\d{2,3}(th)?.Anniversary)(?:.(Cut|Edition|Version))?(.(Extended|Uncensored|Remastered|Unrated|Uncut|Open.?Matte|IMAX|Fan.?Edit))?|((Uncensored|Remastered|Unrated|Uncut|Open?.Matte|IMAX|Fan.?Edit|Restored|((2|3|4)in1))))))\b\)?", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex ReportEditionRegex = new Regex(@"^.+?" + EditionRegex, RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex HardcodedSubsRegex = new Regex(@"\b((?<hcsub>(\w+(?<!SOFT|HORRIBLE)SUBS?))|(?<hc>(HC|SUBBED)))\b",
                                                        RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);

        private static readonly RegexReplace[] PreSubstitutionRegex = Array.Empty<RegexReplace>();

        private static readonly Regex[] ReportTitleRegex = new[]
        {
            // SCENE - Site title in brackets with full year in date then episode info
            // [Site] 19-07-2023 - Loli - Beautiful Episode 2160p {RlsGroup}
            new Regex("^\\[(?<studiotitle>.+?)\\][-_. ]+(?<airday>[0-3][0-9])(?![-_. ]+[0-3][0-9])?[-_. ]+(?<airmonth>[0-1][0-9])[-_. ]+(?<airyear>(19|20)\\d{2})",
                RegexOptions.IgnoreCase | RegexOptions.Compiled),

            // SCENE - Site title in brackets, date after title and performer
            // [Site] Beautiful Episode - Loli - 2023-07-22 - 1080p
            new Regex("^\\[(?<studiotitle>.+?)\\](?<releasetoken>.+?)(?:( - |\\s)(\\[|\\()?(?<airyear>(19|20)\\d{2})[-_.](?<airmonth>[0-1][0-9])[-_.](?<airday>[0-3][0-9])(\\]|\\))?)",
                RegexOptions.IgnoreCase | RegexOptions.Compiled),

            // SCENE with non-separated airdate after title (20180428)
            new Regex(@"^(?<studiotitle>.+?)?[-_. ]+(?<airyear>(19|20)\d{2})(?<airmonth>[0-1][0-9])(?<airday>[0-3][0-9])",
            RegexOptions.IgnoreCase | RegexOptions.Compiled),

            // SCENE with airdate after title [studio] title (dd.mm.yyyy)
            new Regex(@"\[(?<studiotitle>.+?)\]+[-_. ]+(?<releasetoken>.+?)(?<airday>[0-3][0-9])\.(?<airmonth>[0-1][0-9])\.(?<airyear>(19|20)\d{2})\)",
                RegexOptions.IgnoreCase | RegexOptions.Compiled),

            // SCENE with airdate after title studio - title (dd.mm.yyyy)
            new Regex(@"(?<studiotitle>.+?)?[-]+(?<releasetoken>.+?)\((?<airday>[0-3][0-9])\.(?<airmonth>[0-1][0-9])\.(?<airyear>(19|20)\d{2})\)",
                RegexOptions.IgnoreCase | RegexOptions.Compiled),

            // SCENE with airdate (18.04.28, 2018.04.28, 18-04-28, 18 04 28, 18_04_28)
            new Regex(@"^(?<studiotitle>.+?)?[-_. ]+(?<airyear>\d{2}|\d{4})[-_. ]+(?<airmonth>[0-1][0-9])[-_. ]+(?<airday>[0-3][0-9])",
                RegexOptions.IgnoreCase | RegexOptions.Compiled),

            // SCENE with airdate before title (2018-10-12, 20181012) (Strict pattern to avoid false matches)
            new Regex(@"^(?<airyear>19[6-9]\d|20\d{2})[-_]?(?<airmonth>[0-1][0-9])[-_]?(?<airday>[0-3][0-9])",
                RegexOptions.IgnoreCase | RegexOptions.Compiled),

            // Anime [Subgroup] and Year
            new Regex(@"^(?:\[(?<subgroup>.+?)\][-_. ]?)(?<title>(?![(\[]).+?)?(?:(?:[-_\W](?<![)\[!]))*(?<year>(1(8|9)|20)\d{2}(?!p|i|x|\d+|\]|\W\d+)))+.*?(?<hash>\[\w{8}\])?(?:$|\.)", RegexOptions.IgnoreCase | RegexOptions.Compiled),

            // Anime [Subgroup] no year, versioned title, hash
            new Regex(@"^(?:\[(?<subgroup>.+?)\][-_. ]?)(?<title>(?![(\[]).+?)((v)(?:\d{1,2})(?:([-_. ])))(\[.*)?(?:[\[(][^])])?.*?(?<hash>\[\w{8}\])(?:$|\.)", RegexOptions.IgnoreCase | RegexOptions.Compiled),

            // Anime [Subgroup] no year, info in double sets of brackets, hash
            new Regex(@"^(?:\[(?<subgroup>.+?)\][-_. ]?)(?<title>(?![(\[]).+?)(\[.*).*?(?<hash>\[\w{8}\])(?:$|\.)", RegexOptions.IgnoreCase | RegexOptions.Compiled),

            // Anime [Subgroup] no year, info in parentheses or brackets, hash
            new Regex(@"^(?:\[(?<subgroup>.+?)\][-_. ]?)(?<title>(?![(\[]).+)(?:[\[(][^])]).*?(?<hash>\[\w{8}\])(?:$|\.)", RegexOptions.IgnoreCase | RegexOptions.Compiled),

            // Some german or french tracker formats (missing year, ...) (Only applies to german and TrueFrench releases) - see ParserFixture for examples and tests - french removed as it broke all movies w/ french titles
            new Regex(@"^(?<title>(?![(\[]).+?)((\W|_))(" + EditionRegex + @".{1,3})?(?:(?<!(19|20)\d{2}.*?)(?<!(?:Good|The)[_ .-])(German|TrueFrench))(.+?)(?=((19|20)\d{2}|$))(?<year>(19|20)\d{2}(?!p|i|\d+|\]|\W\d+))?(\W+|_|$)(?!\\)", RegexOptions.IgnoreCase | RegexOptions.Compiled),

            // Special, Despecialized, etc. Edition Movies, e.g: Mission.Impossible.3.Special.Edition.2011
            new Regex(@"^(?<title>(?![(\[]).+?)?(?:(?:[-_\W](?<![)\[!]))*" + EditionRegex + @".{1,3}(?<year>(1(8|9)|20)\d{2}(?!p|i|\d+|\]|\W\d+)))+(\W+|_|$)(?!\\)",
                          RegexOptions.IgnoreCase | RegexOptions.Compiled),

            // Special, Despecialized, etc. Edition Movies, e.g: Mission.Impossible.3.2011.Special.Edition //TODO: Seems to slow down parsing heavily!
            /*new Regex(@"^(?<title>(?![(\[]).+?)?(?:(?:[-_\W](?<![)\[!]))*(?<year>(19|20)\d{2}(?!p|i|(19|20)\d{2}|\]|\W(19|20)\d{2})))+(\W+|_|$)(?!\\)\(?(?<edition>(((Extended.|Ultimate.)?(Director.?s|Collector.?s|Theatrical|Ultimate|Final(?=(.(Cut|Edition|Version)))|Extended|Rogue|Special|Despecialized|\d{2,3}(th)?.Anniversary)(.(Cut|Edition|Version))?(.(Extended|Uncensored|Remastered|Unrated|Uncut|IMAX|Fan.?Edit))?|((Uncensored|Remastered|Unrated|Uncut|IMAX|Fan.?Edit|Edition|Restored|((2|3|4)in1))))))\)?",
                          RegexOptions.IgnoreCase | RegexOptions.Compiled),*/

            // Normal movie format, e.g: Mission.Impossible.3.2011
            new Regex(@"^(?<title>(?![(\[]).+?)?(?:(?:[-_\W](?<![)\[!]))*(?<year>(1(8|9)|20)\d{2}(?!p|i|(1(8|9)|20)\d{2}|\]|\W(1(8|9)|20)\d{2})))+(\W+|_|$)(?!\\)", RegexOptions.IgnoreCase | RegexOptions.Compiled),

            // PassThePopcorn Torrent names: Star.Wars[PassThePopcorn]
            new Regex(@"^(?<title>.+?)?(?:(?:[-_\W](?<![()\[!]))*(?<year>(\[\w *\])))+(\W+|_|$)(?!\\)", RegexOptions.IgnoreCase | RegexOptions.Compiled),

            // That did not work? Maybe some tool uses [] for years. Who would do that?
            new Regex(@"^(?<title>(?![(\[]).+?)?(?:(?:[-_\W](?<![)!]))*(?<year>(1(8|9)|20)\d{2}(?!p|i|\d+|\W\d+)))+(\W+|_|$)(?!\\)", RegexOptions.IgnoreCase | RegexOptions.Compiled),

            // SCENE with Episode numbers after studio E1234 title
            new Regex(@"(?<studiotitle>.+?)?[-_. ]+(?<episode>[eE]+\d{1,6})",
                RegexOptions.IgnoreCase | RegexOptions.Compiled),

            // As a last resort for movies that have ( or [ in their title.
            new Regex(@"^(?<title>.+?)?(?:(?:[-_\W](?<![)\[!]))*(?<year>(1(8|9)|20)\d{2}(?!p|i|\d+|\]|\W\d+)))+(\W+|_|$)(?!\\)", RegexOptions.IgnoreCase | RegexOptions.Compiled),

            new Regex(@"^(?<title>(?![(\[]).+?)?(XXX)(\W+|_|$)(?!\\)", RegexOptions.IgnoreCase | RegexOptions.Compiled),
        };

        private static readonly Regex[] ReportTitleFolderRegex = new[]
        {
            // When year comes first.
            new Regex(@"^(?:(?:[-_\W](?<![)!]))*(?<year>(19|20)\d{2}(?!p|i|\d+|\W\d+)))+(\W+|_|$)(?<title>.+?)?$")
        };

        private static readonly Regex[] RejectHashedReleasesRegex = new Regex[]
            {
                // Generic match for md5 and mixed-case hashes.
                new Regex(@"^[0-9a-zA-Z]{32}", RegexOptions.Compiled),

                // Generic match for shorter lower-case hashes.
                new Regex(@"^[a-z0-9]{24}$", RegexOptions.Compiled),

                // Format seen on some NZBGeek releases
                // Be very strict with these coz they are very close to the valid 101 ep numbering.
                new Regex(@"^[A-Z]{11}\d{3}$", RegexOptions.Compiled),
                new Regex(@"^[a-z]{12}\d{3}$", RegexOptions.Compiled),

                // Backup filename (Unknown origins)
                new Regex(@"^Backup_\d{5,}S\d{2}-\d{2}$", RegexOptions.Compiled),

                // 123 - Started appearing December 2014
                new Regex(@"^123$", RegexOptions.Compiled),

                // abc - Started appearing January 2015
                new Regex(@"^abc$", RegexOptions.Compiled | RegexOptions.IgnoreCase),

                // abc - Started appearing 2020
                new Regex(@"^abc[-_. ]xyz", RegexOptions.Compiled | RegexOptions.IgnoreCase),

                // b00bs - Started appearing January 2015
                new Regex(@"^b00bs$", RegexOptions.Compiled | RegexOptions.IgnoreCase)
            };

        // Regex to detect whether the title was reversed.
        private static readonly Regex ReversedTitleRegex = new Regex(@"(?:^|[-._ ])(p027|p0801)[-._ ]", RegexOptions.Compiled);

        // Regex to split movie titles that contain `AKA`.
        private static readonly Regex AlternativeTitleRegex = new Regex(@"[ ]+(?:AKA|\/)[ ]+", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        // Regex to unbracket alternative titles.
        private static readonly Regex BracketedAlternativeTitleRegex = new Regex(@"(.*) \([ ]*AKA[ ]+(.*)\)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex NormalizeRegex = new Regex(@"((?:\b|_)(?<!^|[^a-zA-Z0-9_']\w[^a-zA-Z0-9_'])(a(?!$|[^a-zA-Z0-9_']\w[^a-zA-Z0-9_'])|an|the|and|or|of)(?!$)(?:\b|_))|\W|_",
                                                                RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex FileExtensionRegex = new Regex(@"\.[a-z0-9]{2,4}$",
                                                                RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex ReportImdbId = new Regex(@"(?<imdbid>tt\d{7,8})", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex ReportTmdbId = new Regex(@"tmdb(id)?-(?<tmdbid>\d+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly RegexReplace SimpleTitleRegex = new RegexReplace(@"(?:(480|540|576|720|1080|2160)[ip]|[xh][\W_]?26[45]|DD\W?5\W1|[<>?*]|848x480|1280x720|1920x1080|3840x2160|4096x2160|(8|10)b(it)?|10-bit)\s*?(?![a-b0-9])",
                                                                string.Empty,
                                                                RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex SpecialEpisodeTitleRegex = new Regex(@"(?<episodetitle>.+?)(?:\[.*(?:480p|720p|1080p|2160p|HDTV|WEB|WEBRip|WEB-?DL).*\]|[. ]XXX[. ](?:480p|720p|1080p|2160p|HDTV|WEB|WEBRip|WEB-?DL).*|(?:480p|720p|1080p|2160p|HDTV|WEB|WEBRip|WEB-?DL)|$)",
                          RegexOptions.Compiled);

        private static readonly Regex SimpleReleaseTitleRegex = new Regex(@"\s*(?:[<>?*|])", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        // Valid TLDs http://data.iana.org/TLD/tlds-alpha-by-domain.txt
        private static readonly RegexReplace WebsitePrefixRegex = new RegexReplace(@"^(?:\[\s*)?(?:www\.)?[-a-z0-9-]{1,256}\.(?<!Naruto-Kun\.)(?:[a-z]{2,6}\.[a-z]{2,6}|xn--[a-z0-9-]{4,}|[a-z]{2,})\b(?:\s*\]|[ -]{2,})[ -]*",
                                                                string.Empty,
                                                                RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly RegexReplace WebsitePostfixRegex = new RegexReplace(@"(?:\[\s*)?(?:www\.)?[-a-z0-9-]{1,256}\.(?:xn--[a-z0-9-]{4,}|[a-z]{2,6})\b(?:\s*\])$",
                                                        string.Empty,
                                                        RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly RegexReplace CleanReleaseGroupRegex = new RegexReplace(@"(-(RP|1|NZBGeek|Obfuscated|Obfuscation|Scrambled|sample|Pre|postbot|xpost|Rakuv[a-z0-9]*|WhiteRev|BUYMORE|AsRequested|AlternativeToRequested|GEROV|Z0iDS3N|Chamele0n|4P|4Planet|AlteZachen|RePACKPOST))+$",
                                                                string.Empty,
                                                                RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly RegexReplace CleanTorrentSuffixRegex = new RegexReplace(@"\[(?:ettv|rartv|rarbg|cttv|publichd)\]$",
                                                                string.Empty,
                                                                RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex CleanQualityBracketsRegex = new Regex(@"\[[a-z0-9 ._-]+\]$",
                                                                   RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex ReleaseGroupRegex = new Regex(@"-(?<releasegroup>[a-z0-9]+(?<part2>-[a-z0-9]+)?(?!.+?(?:480p|576p|720p|1080p|2160p)))(?<!(?:WEB-(DL|Rip)|Blu-Ray|480p|576p|720p|1080p|2160p|DTS-HD|DTS-X|DTS-MA|DTS-ES|-ES|-EN|-CAT|-HDRip|\d{1,2}-bit|[ ._]\d{4}-\d{2}|-\d{2}|tmdb(id)?-(?<tmdbid>\d+)|(?<imdbid>tt\d{7,8}))(?:\k<part2>)?)(?:\b|[-._ ]|$)|[-._ ]\[(?<releasegroup>[a-z0-9]+)\]$",
                                                                RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex InvalidReleaseGroupRegex = new Regex(@"^([se]\d+|[0-9a-f]{8})$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex AnimeReleaseGroupRegex = new Regex(@"^(?:\[(?<subgroup>(?!\s).+?(?<!\s))\](?:_|-|\s|\.)?)",
                                                                RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex YearInTitleRegex = new Regex(@"^(?<title>.+?)(?:\W|_.)?[\(\[]?(?<year>\d{4})[\]\)]?",
                                                                RegexOptions.IgnoreCase | RegexOptions.Compiled);

        // Handle Exception Release Groups that don't follow -RlsGrp; Manual List
        // groups whose releases end with RlsGroup) or RlsGroup]
        private static readonly Regex ExceptionReleaseGroupRegex = new Regex(@"(?<releasegroup>(Silence|afm72|Panda|Ghost|MONOLITH|Tigole|Joy|ImE|UTR|t3nzin|Anime Time|Project Angel|Hakata Ramen|HONE|Vyndros|SEV|Garshasp|Kappa|Natty|RCVR|SAMPA|YOGI|r00t|EDGE2020|RZeroX|FreetheFish|Anna|Bandi|Qman|theincognito|HDO|DusIctv|DHD|CtrlHD|-ZR-|ADC|XZVN|RH|Kametsu|Garshasp)(?=\]|\)))", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        // Handle Exception Release Groups that don't follow -RlsGrp; Manual List
        // name only...BE VERY CAREFUL WITH THIS, HIGH CHANCE OF FALSE POSITIVES
        private static readonly Regex ExceptionReleaseGroupRegexExact = new Regex(@"(?<releasegroup>KRaLiMaRKo|E\.N\.D|D\-Z0N3|Koten_Gars|BluDragon|ZØNEHD|Tigole|HQMUX|VARYG|YIFY|YTS(.(MX|LT|AG))?)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex WordDelimiterRegex = new Regex(@"(\s|\.|,|_|-|=|'|\|)+", RegexOptions.Compiled);
        private static readonly Regex SpecialCharRegex = new Regex(@"(\&|\:|\\|\/)+", RegexOptions.Compiled);
        private static readonly Regex PunctuationRegex = new Regex(@"[^\w\s]", RegexOptions.Compiled);
        private static readonly Regex CommonWordRegex = new Regex(@"\b(a|an|the|and|or|of)\b\s?", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex SpecialEpisodeWordRegex = new Regex(@"\b(part|special|edition|christmas)\b\s?", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex DuplicateSpacesRegex = new Regex(@"\s{2,}", RegexOptions.Compiled);

        private static readonly Regex RequestInfoRegex = new Regex(@"^(?:\[.+?\])+", RegexOptions.Compiled);

        private static readonly string[] Numbers = new[] { "zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine" };
        private static Dictionary<string, string> _umlautMappings = new Dictionary<string, string>
        {
            { "ö", "oe" },
            { "ä", "ae" },
            { "ü", "ue" },
        };

        public static ParsedMovieInfo ParseMoviePath(string path)
        {
            var fileInfo = new FileInfo(path);

            var result = ParseMovieTitle(fileInfo.Name, true);

            if (result == null)
            {
                Logger.Debug("Attempting to parse movie info using directory and file names. {0}", fileInfo.Directory.Name);
                result = ParseMovieTitle(fileInfo.Directory.Name + " " + fileInfo.Name);
            }

            if (result == null)
            {
                Logger.Debug("Attempting to parse movie info using directory name. {0}", fileInfo.Directory.Name);
                result = ParseMovieTitle(fileInfo.Directory.Name + fileInfo.Extension);
            }

            return result;
        }

        public static ParsedMovieInfo ParseMovieTitle(string title, bool isDir = false)
        {
            var originalTitle = title;
            try
            {
                if (!ValidateBeforeParsing(title))
                {
                    return null;
                }

                Logger.Debug("Parsing string '{0}'", title);

                if (ReversedTitleRegex.IsMatch(title))
                {
                    var titleWithoutExtension = RemoveFileExtension(title).ToCharArray();
                    Array.Reverse(titleWithoutExtension);

                    title = $"{titleWithoutExtension}{title.Substring(titleWithoutExtension.Length)}";

                    Logger.Debug("Reversed name detected. Converted to '{0}'", title);
                }

                var releaseTitle = RemoveFileExtension(title);

                // Trim dashes from end
                releaseTitle = releaseTitle.Trim('-', '_');

                releaseTitle = releaseTitle.Replace("【", "[").Replace("】", "]");

                foreach (var replace in PreSubstitutionRegex)
                {
                    if (replace.TryReplace(ref releaseTitle))
                    {
                        Logger.Trace($"Replace regex: {replace}");
                        Logger.Debug("Substituted with " + releaseTitle);
                    }
                }

                var simpleTitle = SimpleTitleRegex.Replace(releaseTitle);

                // TODO: Quick fix stripping [url] - prefixes.
                simpleTitle = WebsitePrefixRegex.Replace(simpleTitle);
                simpleTitle = WebsitePostfixRegex.Replace(simpleTitle);

                simpleTitle = CleanTorrentSuffixRegex.Replace(simpleTitle);

                simpleTitle = CleanQualityBracketsRegex.Replace(simpleTitle, m =>
                {
                    if (QualityParser.ParseQualityName(m.Value).Quality != Qualities.Quality.Unknown)
                    {
                        return string.Empty;
                    }

                    return m.Value;
                });

                var allRegexes = ReportTitleRegex.ToList();

                if (isDir)
                {
                    allRegexes.AddRange(ReportTitleFolderRegex);
                }

                foreach (var regex in allRegexes)
                {
                    var match = regex.Matches(simpleTitle);
                    if (match.Count == 0)
                    {
                        match = regex.Matches(originalTitle);
                    }

                    if (match.Count != 0)
                    {
                        Logger.Trace(regex);
                        try
                        {
                            var result = ParseMatchCollection(match, releaseTitle);

                            if (result != null)
                            {
                                // TODO: Add tests for this!
                                var simpleReleaseTitle = SimpleReleaseTitleRegex.Replace(releaseTitle, string.Empty);

                                var simpleTitleReplaceString = match[0].Groups["title"].Success ? match[0].Groups["title"].Value : result.PrimaryMovieTitle;

                                if (simpleTitleReplaceString.IsNotNullOrWhiteSpace())
                                {
                                    if (match[0].Groups["title"].Success)
                                    {
                                        simpleReleaseTitle = simpleReleaseTitle.Remove(match[0].Groups["title"].Index, match[0].Groups["title"].Length)
                                                                               .Insert(match[0].Groups["title"].Index, simpleTitleReplaceString.Contains('.') ? "A.Movie" : "A Movie");
                                    }
                                    else
                                    {
                                        simpleReleaseTitle = simpleReleaseTitle.Replace(simpleTitleReplaceString, simpleTitleReplaceString.Contains('.') ? "A.Movie" : "A Movie");
                                    }
                                }

                                result.ReleaseGroup = ParseReleaseGroup(simpleReleaseTitle);

                                var subGroup = GetSubGroup(match);
                                if (!subGroup.IsNullOrWhiteSpace())
                                {
                                    result.ReleaseGroup = subGroup;
                                }

                                result.HardcodedSubs = ParseHardcodeSubs(title);

                                Logger.Debug("Release Group parsed: {0}", result.ReleaseGroup);

                                result.Languages = LanguageParser.ParseLanguages(result.ReleaseGroup.IsNotNullOrWhiteSpace() ? simpleReleaseTitle.Replace(result.ReleaseGroup, "RlsGrp") : simpleReleaseTitle);
                                Logger.Debug("Languages parsed: {0}", string.Join(", ", result.Languages));

                                result.Quality = QualityParser.ParseQuality(title);
                                Logger.Debug("Quality parsed: {0}", result.Quality);

                                if (result.Edition.IsNullOrWhiteSpace())
                                {
                                    result.Edition = ParseEdition(simpleReleaseTitle);
                                    Logger.Debug("Edition parsed: {0}", result.Edition);
                                }

                                result.ReleaseHash = GetReleaseHash(match);
                                if (!result.ReleaseHash.IsNullOrWhiteSpace())
                                {
                                    Logger.Debug("Release Hash parsed: {0}", result.ReleaseHash);
                                }

                                result.OriginalTitle = originalTitle;
                                result.ReleaseTitle = releaseTitle;
                                result.SimpleReleaseTitle = simpleReleaseTitle;

                                result.ImdbId = ParseImdbId(simpleReleaseTitle);
                                result.TmdbId = ParseTmdbId(simpleReleaseTitle);

                                return result;
                            }
                        }
                        catch (InvalidDateException ex)
                        {
                            Logger.Debug(ex, ex.Message);
                            break;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                if (!title.ToLower().Contains("password") && !title.ToLower().Contains("yenc"))
                {
                    Logger.Error(e, "An error has occurred while trying to parse {0}", title);
                }
            }

            Logger.Debug("Unable to parse {0}", title);
            return null;
        }

        public static string ParseImdbId(string title)
        {
            if (title.IsNullOrWhiteSpace())
            {
                return string.Empty;
            }

            var match = ReportImdbId.Match(title);
            if (match.Success)
            {
                if (match.Groups["imdbid"].Value != null)
                {
                    if (match.Groups["imdbid"].Length == 9 || match.Groups["imdbid"].Length == 10)
                    {
                        return match.Groups["imdbid"].Value;
                    }
                }
            }

            return "";
        }

        public static int ParseTmdbId(string title)
        {
            if (title.IsNullOrWhiteSpace())
            {
                return 0;
            }

            var match = ReportTmdbId.Match(title);
            if (match.Success)
            {
                if (match.Groups["tmdbid"].Value != null)
                {
                    return int.TryParse(match.Groups["tmdbid"].Value, out var tmdbId) ? tmdbId : 0;
                }
            }

            return 0;
        }

        public static string ParseEdition(string languageTitle)
        {
            if (languageTitle.IsNullOrWhiteSpace())
            {
                return string.Empty;
            }

            var editionMatch = ReportEditionRegex.Match(languageTitle);

            if (editionMatch.Success && editionMatch.Groups["edition"].Value != null &&
                editionMatch.Groups["edition"].Value.IsNotNullOrWhiteSpace())
            {
                return editionMatch.Groups["edition"].Value.Replace(".", " ");
            }

            return "";
        }

        public static string ReplaceGermanUmlauts(string s)
        {
            if (s.IsNullOrWhiteSpace())
            {
                return string.Empty;
            }

            var t = s;
            t = t.Replace("ä", "ae");
            t = t.Replace("ö", "oe");
            t = t.Replace("ü", "ue");
            t = t.Replace("Ä", "Ae");
            t = t.Replace("Ö", "Oe");
            t = t.Replace("Ü", "Ue");
            t = t.Replace("ß", "ss");
            return t;
        }

        public static string NormalizeImdbId(string imdbId)
        {
            var imdbRegex = new Regex(@"^(\d{1,10}|(tt)\d{1,10})$");

            if (!imdbRegex.IsMatch(imdbId))
            {
                return null;
            }

            if (imdbId.Length > 2)
            {
                imdbId = imdbId.Replace("tt", "").PadLeft(7, '0');
                return $"tt{imdbId}";
            }

            return null;
        }

        public static string ToUrlSlug(string value, bool invalidDashReplacement = false, string trimEndChars = "-_", string deduplicateChars = "-_")
        {
            // First to lower case
            value = value.ToLowerInvariant();

            // Remove all accents
            value = value.RemoveAccent();

            // Replace spaces
            value = Regex.Replace(value, @"\s", "-", RegexOptions.Compiled);

            // Should invalid characters be replaced with dash or empty string?
            var replaceCharacter = invalidDashReplacement ? "-" : string.Empty;

            // Remove invalid chars
            value = Regex.Replace(value, @"[^a-z0-9\s-_]", replaceCharacter, RegexOptions.Compiled);

            // Trim dashes or underscores from end, or user defined character set
            if (!string.IsNullOrEmpty(trimEndChars))
            {
                value = value.Trim(trimEndChars.ToCharArray());
            }

            // Replace double occurrences of - or _, or user defined character set
            if (!string.IsNullOrEmpty(deduplicateChars))
            {
                value = Regex.Replace(value, @"([" + deduplicateChars + "]){2,}", "$1", RegexOptions.Compiled);
            }

            return value;
        }

        public static string CleanStudioTitle(this string title)
        {
            if (title.IsNotNullOrWhiteSpace())
            {
                return title.Replace(" ", "").CleanMovieTitle();
            }

            return string.Empty;
        }

        public static string CleanMovieTitle(this string title)
        {
            if (title.IsNullOrWhiteSpace())
            {
                return string.Empty;
            }

            // If Title only contains numbers return it as is.
            if (long.TryParse(title, out _))
            {
                return title;
            }

            return ReplaceGermanUmlauts(NormalizeRegex.Replace(title, string.Empty).ToLower()).RemoveAccent();
        }

        public static string CleanEpisodeTitle(this string title)
        {
            if (title.IsNullOrWhiteSpace())
            {
                return string.Empty;
            }

            var allRegexes = ReportTitleRegex.ToList();

            foreach (var regex in allRegexes)
            {
                if (title.IsNullOrWhiteSpace())
                {
                    return string.Empty;
                }

                var match = regex.Matches(title);

                if (match.Count != 0)
                {
                    var result = ParseMatchCollection(match, title);

                    if (result != null)
                    {
                        var simpleReleaseTitle = SimpleReleaseTitleRegex.Replace(title, string.Empty);

                        title = result.PrimaryMovieTitle;
                    }
                }
            }

            return title;
        }

        public static string NormalizeEpisodeTitle(this string title)
        {
            if (title.IsNullOrWhiteSpace())
            {
                return string.Empty;
            }

            title = SpecialEpisodeWordRegex.Replace(title, string.Empty);
            title = PunctuationRegex.Replace(title, " ");
            title = DuplicateSpacesRegex.Replace(title, " ");

            return title.Trim()
                        .ToLower();
        }

        public static string NormalizeTitle(this string title)
        {
            if (title.IsNullOrWhiteSpace())
            {
                return string.Empty;
            }

            title = WordDelimiterRegex.Replace(title, " ");
            title = PunctuationRegex.Replace(title, string.Empty);
            title = CommonWordRegex.Replace(title, string.Empty);
            title = DuplicateSpacesRegex.Replace(title, " ");
            title = SpecialCharRegex.Replace(title, string.Empty);

            return title.Trim().ToLower();
        }

        public static string SimplifyReleaseTitle(this string title)
        {
            if (title.IsNullOrWhiteSpace())
            {
                return string.Empty;
            }

            return SimpleReleaseTitleRegex.Replace(title, string.Empty);
        }

        public static string TrimAtEnd(this string title, string textToTrim)
        {
            if (title.IsNullOrWhiteSpace())
            {
                return string.Empty;
            }

            if (title.EndsWith(textToTrim))
            {
                title = title.Remove(title.Length - textToTrim.Length);
            }

            return title.Trim();
        }

        public static string ParseHardcodeSubs(string title)
        {
            if (title.IsNullOrWhiteSpace())
            {
                return string.Empty;
            }

            var subMatch = HardcodedSubsRegex.Matches(title).OfType<Match>().LastOrDefault();

            if (subMatch != null && subMatch.Success)
            {
                if (subMatch.Groups["hcsub"].Success)
                {
                    return subMatch.Groups["hcsub"].Value;
                }
                else if (subMatch.Groups["hc"].Success)
                {
                    return "Generic Hardcoded Subs";
                }
            }

            return null;
        }

        public static string ParseReleaseGroup(string title)
        {
            if (title.IsNullOrWhiteSpace())
            {
                return string.Empty;
            }

            title = title.Trim();
            title = RemoveFileExtension(title);
            title = WebsitePrefixRegex.Replace(title);
            title = CleanTorrentSuffixRegex.Replace(title);

            var animeMatch = AnimeReleaseGroupRegex.Match(title);

            if (animeMatch.Success)
            {
                return animeMatch.Groups["subgroup"].Value;
            }

            title = CleanReleaseGroupRegex.Replace(title);

            var exceptionReleaseGroupRegex = ExceptionReleaseGroupRegex.Matches(title);

            if (exceptionReleaseGroupRegex.Count != 0)
            {
                return exceptionReleaseGroupRegex.OfType<Match>().Last().Groups["releasegroup"].Value;
            }

            var exceptionExactMatch = ExceptionReleaseGroupRegexExact.Matches(title);

            if (exceptionExactMatch.Count != 0)
            {
                return exceptionExactMatch.OfType<Match>().Last().Groups["releasegroup"].Value;
            }

            var matches = ReleaseGroupRegex.Matches(title);

            if (matches.Count != 0)
            {
                var group = matches.OfType<Match>().Last().Groups["releasegroup"].Value;

                if (int.TryParse(group, out _))
                {
                    return null;
                }

                if (InvalidReleaseGroupRegex.IsMatch(group))
                {
                    return null;
                }

                return group;
            }

            return null;
        }

        public static string RemoveFileExtension(string title)
        {
            if (title.IsNullOrWhiteSpace())
            {
                return string.Empty;
            }

            title = FileExtensionRegex.Replace(title, m =>
            {
                var extension = m.Value.ToLower();
                if (MediaFiles.MediaFileExtensions.Extensions.Contains(extension) || new[] { ".par2", ".nzb" }.Contains(extension))
                {
                    return string.Empty;
                }

                return m.Value;
            });

            return title;
        }

        private static ParsedMovieInfo ParseMatchCollection(MatchCollection matchCollection, string releaseTitle)
        {
            if (!matchCollection[0].Groups["airyear"].Success && !matchCollection[0].Groups["episode"].Success)
            {
                if (!matchCollection[0].Groups["title"].Success || matchCollection[0].Groups["title"].Value == "(")
                {
                    return null;
                }

                var movieName = matchCollection[0].Groups["title"].Value.Replace('_', ' ');
                movieName = RequestInfoRegex.Replace(movieName, "").Trim(' ');

                var parts = movieName.Split('.');
                movieName = "";
                var n = 0;
                var previousAcronym = false;
                var nextPart = "";
                foreach (var part in parts)
                {
                    if (parts.Length >= n + 2)
                    {
                        nextPart = parts[n + 1];
                    }
                    else
                    {
                        nextPart = "";
                    }

                    if (part.Length == 1 && part.ToLower() != "a" && !int.TryParse(part, out _) &&
                        (previousAcronym || n < parts.Length - 1) &&
                        (previousAcronym || nextPart.Length != 1 || !int.TryParse(nextPart, out _)))
                    {
                        movieName += part + ".";
                        previousAcronym = true;
                    }
                    else if (part.ToLower() == "a" && (previousAcronym || nextPart.Length == 1))
                    {
                        movieName += part + ".";
                        previousAcronym = true;
                    }
                    else if (part.ToLower() == "dr")
                    {
                        movieName += part + ".";
                        previousAcronym = true;
                    }
                    else
                    {
                        if (previousAcronym)
                        {
                            movieName += " ";
                            previousAcronym = false;
                        }

                        movieName += part + " ";
                    }

                    n++;
                }

                movieName = movieName.Trim(' ').TrimEnd("XXX").Trim(' ');

                int.TryParse(matchCollection[0].Groups["year"].Value, out var releaseYear);

                ParsedMovieInfo result;

                result = new ParsedMovieInfo { Year = releaseYear };

                if (matchCollection[0].Groups["edition"].Success)
                {
                    result.Edition = matchCollection[0].Groups["edition"].Value.Replace(".", " ");
                }

                var movieTitles = new List<string>();
                movieTitles.Add(movieName);

                // Delete parentheses of the form (aka ...).
                var unbracketedName = BracketedAlternativeTitleRegex.Replace(movieName, "$1 AKA $2");

                // Split by AKA and filter out empty and duplicate names.
                movieTitles
                    .AddRange(AlternativeTitleRegex
                            .Split(unbracketedName)
                            .Where(alternativeName => alternativeName.IsNotNullOrWhiteSpace() && alternativeName != movieName));

                result.MovieTitles = movieTitles;

                Logger.Debug("Movie Parsed. {0}", result);

                return result;
            }
            else
            {
                var result = new ParsedMovieInfo
                {
                    ReleaseTitle = releaseTitle,
                };

                if (matchCollection[0].Groups["airyear"].Success)
                {
                    int.TryParse(matchCollection[0].Groups["airyear"].Value, out var airYear);

                    if (airYear <= 99)
                    {
                        airYear = CultureInfo.CurrentCulture.Calendar.ToFourDigitYear(airYear);
                    }

                    // Try to Parse as a daily show
                    var airmonth = Convert.ToInt32(matchCollection[0].Groups["airmonth"].Value);
                    var airday = Convert.ToInt32(matchCollection[0].Groups["airday"].Value);

                    // Swap day and month if month is bigger than 12 (scene fail)
                    if (airmonth > 12)
                    {
                        var tempDay = airday;
                        airday = airmonth;
                        airmonth = tempDay;
                    }

                    DateTime airDate;

                    try
                    {
                        airDate = new DateTime(airYear, airmonth, airday);
                    }
                    catch (Exception)
                    {
                        throw new InvalidDateException("Invalid date found: {0}-{1}-{2}", airYear, airmonth, airday);
                    }

                    // Check if episode is in the future (most likely a parse error)
                    if (airDate > DateTime.Now.AddDays(1).Date)
                    {
                        throw new InvalidDateException("Invalid date found: {0}", airDate);
                    }

                    // If the parsed air date is before 1970 and the title year wasn't matched (not a match for the Plex DVR format) throw an error
                    if (airDate < new DateTime(1970, 1, 1) && matchCollection[0].Groups["titleyear"].Value.IsNullOrWhiteSpace())
                    {
                        throw new InvalidDateException("Invalid date found: {0}", airDate);
                    }

                    result.ReleaseDate = airDate.ToString(Movie.RELEASE_DATE_FORMAT);
                }

                var studioTitle = matchCollection[0].Groups["studiotitle"].Value.TrimAtEnd(".com").Replace('.', ' ').Replace('_', ' ');
                studioTitle = RequestInfoRegex.Replace(studioTitle, "").Trim(' ');

                var lastSeasonEpisodeStringIndex = matchCollection[0].Groups["studiotitle"].EndIndex();

                if (result.ReleaseDate.IsNotNullOrWhiteSpace())
                {
                    lastSeasonEpisodeStringIndex = Math.Max(lastSeasonEpisodeStringIndex, matchCollection[0].Groups["airyear"].EndIndex());
                    lastSeasonEpisodeStringIndex = Math.Max(lastSeasonEpisodeStringIndex, matchCollection[0].Groups["airmonth"].EndIndex());
                    lastSeasonEpisodeStringIndex = Math.Max(lastSeasonEpisodeStringIndex, matchCollection[0].Groups["airday"].EndIndex());
                }

                if (matchCollection[0].Groups["episode"].Success)
                {
                    result.Episode = matchCollection[0].Groups["episode"].Value;

                    lastSeasonEpisodeStringIndex = Math.Max(lastSeasonEpisodeStringIndex, matchCollection[0].Groups["episode"].EndIndex());
                }

                if (matchCollection[0].Groups["releasetoken"].Success)
                {
                    result.ReleaseTokens = matchCollection[0].Groups["releasetoken"].Value;
                }

                if (result.ReleaseTokens.IsNullOrWhiteSpace())
                {
                    var releaseTokens = releaseTitle;

                    if (lastSeasonEpisodeStringIndex != releaseTitle.Length)
                    {
                        releaseTokens = releaseTitle.Substring(lastSeasonEpisodeStringIndex);
                    }

                    var match = SpecialEpisodeTitleRegex.Match(releaseTokens);

                    if (match != null && match.Groups["episodetitle"].Value.IsNotNullOrWhiteSpace())
                    {
                        releaseTokens = match.Groups["episodetitle"].Value;
                    }

                    result.ReleaseTokens = releaseTokens;
                }

                result.StudioTitle = studioTitle;

                Logger.Debug("Scene Parsed. {0}", result);

                return result;
            }
        }

        private static bool ValidateBeforeParsing(string title)
        {
            if (title.ToLower().Contains("password") && title.ToLower().Contains("yenc"))
            {
                Logger.Debug("");
                return false;
            }

            if (!title.Any(char.IsLetterOrDigit))
            {
                return false;
            }

            var titleWithoutExtension = RemoveFileExtension(title);

            if (RejectHashedReleasesRegex.Any(v => v.IsMatch(titleWithoutExtension)))
            {
                Logger.Debug("Rejected Hashed Release Title: " + title);
                return false;
            }

            return true;
        }

        private static string GetSubGroup(MatchCollection matchCollection)
        {
            var subGroup = matchCollection[0].Groups["subgroup"];

            if (subGroup.Success)
            {
                return subGroup.Value;
            }

            return string.Empty;
        }

        private static string GetReleaseHash(MatchCollection matchCollection)
        {
            var hash = matchCollection[0].Groups["hash"];

            if (hash.Success)
            {
                var hashValue = hash.Value.Trim('[', ']');

                if (hashValue.Equals("1280x720"))
                {
                    return string.Empty;
                }

                return hashValue;
            }

            return string.Empty;
        }
    }
}
