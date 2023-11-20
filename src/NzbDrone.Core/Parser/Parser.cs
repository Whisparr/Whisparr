using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Instrumentation;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Parser
{
    public static class Parser
    {
        private static readonly Logger Logger = NzbDroneLogger.GetLogger(typeof(Parser));

        private static readonly RegexReplace[] PreSubstitutionRegex = new[]
            {
                // Korean series without season number, replace with S01Exxx and remove airdate
                new RegexReplace(@"\.E(\d{2,4})\.\d{6}\.(.*-NEXT)$", ".S01E$1.$2", RegexOptions.Compiled),

                // Some Chinese anime releases contain both English and Chinese titles, remove the Chinese title and replace with normal anime pattern
                new RegexReplace(@"^\[(?:(?<subgroup>[^\]]+?)(?:[\u4E00-\u9FCC]+)?)\]\[(?<title>[^\]]+?)(?:\s(?<chinesetitle>[\u4E00-\u9FCC][^\]]*?))\]\[(?:(?:[\u4E00-\u9FCC]+?)?(?<episode>\d{1,4})(?:[\u4E00-\u9FCC]+?)?)\]", "[${subgroup}] ${title} - ${episode} - ", RegexOptions.Compiled),

                // Chinese LoliHouse/ZERO/Lilith-Raws releases don't use the expected brackets, normalize using brackets
                new RegexReplace(@"^\[(?<subgroup>[^\]]*?(?:LoliHouse|ZERO|Lilith-Raws)[^\]]*?)\](?<title>[^\[\]]+?)(?: - (?<episode>[0-9-]+)\s*|\[第?(?<episode>[0-9]+(?:-[0-9]+)?)话?(?:END|完)?\])\[", "[${subgroup}][${title}][${episode}][", RegexOptions.Compiled),

                // Most Chinese anime releases contain additional brackets/separators for chinese and non-chinese titles, remove junk and replace with normal anime pattern
                new RegexReplace(@"^\[(?<subgroup>[^\]]+)\](?:\s?★[^\[ -]+\s?)?\[?(?:(?<chinesetitle>(?=[^\]]*?[\u4E00-\u9FCC])[^\]]*?)(?:\]\[|\s*[_/·]\s*)){0,2}(?<title>[^\]]+?)\]?(?:\[\d{4}\])?\[第?(?<episode>[0-9]+(?:-[0-9]+)?)(?:话|集)?(?: ?END|完| ?Fin)?\]", "[${subgroup}] ${title} - ${episode} ", RegexOptions.Compiled),

                // Some Chinese anime releases contain both Chinese and English titles, remove the Chinese title and replace with normal anime pattern
                new RegexReplace(@"^\[(?<subgroup>[^\]]+)\](?:\s)(?:(?<chinesetitle>(?=[^\]]*?[\u4E00-\u9FCC])[^\]]*?)(?:\s/\s))(?<title>[^\]]+?)(?:[- ]+)(?<episode>[0-9]+(?:-[0-9]+)?)话?(?:END|完)?", "[${subgroup}] ${title} - ${episode} ", RegexOptions.Compiled),

                // GM-Team releases with lots of square brackets
                new RegexReplace(@"^\[(?<subgroup>[^\]]+)\](?:(?<chinesubgroup>\[(?=[^\]]*?[\u4E00-\u9FCC])[^\]]*\])+)\[(?<title>[^\]]+?)\](?<junk>\[[^\]]+\])*\[(?<episode>[0-9]+(?:-[0-9]+)?)( END| Fin)?\]", "[${subgroup}] ${title} - ${episode} ", RegexOptions.Compiled)
            };

        private static readonly Regex[] ReportTitleRegex = new[]
            {
                // Site title in brackets with full year in date then episode info
                // [Site] 19-07-2023 - Loli - Beautiful Episode 2160p {RlsGroup}
                new Regex("^\\[(?<title>.+?)\\][-_. ]+(?<airday>[0-3][0-9])(?![-_. ]+[0-3][0-9])?[-_. ]+(?<airmonth>[0-1][0-9])[-_. ]+(?<airyear>(19|20)\\d{2})",
                    RegexOptions.IgnoreCase | RegexOptions.Compiled),

                // Site title in brackets, date after title and performer
                // [Site] Beautiful Episode - Loli - 2023-07-22 - 1080p
                new Regex("^\\[(?<title>.+?)\\](?<releasetoken>.+?)(?:( - |\\s)(\\[|\\()?(?<airyear>(19|20)\\d{2})[-_.](?<airmonth>[0-1][0-9])[-_.](?<airday>[0-3][0-9])(\\]|\\))?)",
                    RegexOptions.IgnoreCase | RegexOptions.Compiled),

                // Episodes with non-separated airdate after title (20180428)
                new Regex(@"^(?<title>.+?)?[-_. ]+(?<airyear>(19|20)\d{2})(?<airmonth>[0-1][0-9])(?<airday>[0-3][0-9])",
                    RegexOptions.IgnoreCase | RegexOptions.Compiled),

                // Episodes with airdate (18.04.28, 2018.04.28, 18-04-28, 18 04 28, 18_04_28)
                new Regex(@"^(?<title>.+?)?[-_. ]+(?<airyear>\d{2}|\d{4})[-_. ]+(?<airmonth>[0-1][0-9])[-_. ]+(?<airday>[0-3][0-9])",
                    RegexOptions.IgnoreCase | RegexOptions.Compiled),

                // Episodes with airdate before title (2018-10-12, 20181012) (Strict pattern to avoid false matches)
                new Regex(@"^(?<airyear>19[6-9]\d|20\d{2})[-_]?(?<airmonth>[0-1][0-9])[-_]?(?<airday>[0-3][0-9])",
                    RegexOptions.IgnoreCase | RegexOptions.Compiled)
            };

        private static readonly Regex[] SpecialEpisodeTitleRegex = new Regex[]
            {
                new Regex(@"(?<episodetitle>.+?)(?:\[.*(?:720p|1080p|2160p|HDTV|WEB|WEBRip|WEB-?DL).*\]|XXX|$)",
                          RegexOptions.IgnoreCase | RegexOptions.Compiled)
            };

        private static readonly Regex[] RejectHashedReleasesRegexes = new Regex[]
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
                new Regex(@"^b00bs$", RegexOptions.Compiled | RegexOptions.IgnoreCase),

                // 170424_26 - Started appearing August 2018
                new Regex(@"^\d{6}_\d{2}$"),

                // additional Generic match for mixed-case hashes. - Started appearing Dec 2020
                new Regex(@"^[0-9a-zA-Z]{30}", RegexOptions.Compiled),

                // additional Generic match for mixed-case hashes. - Started appearing Jan 2021
                new Regex(@"^[0-9a-zA-Z]{26}", RegexOptions.Compiled),

                // additional Generic match for mixed-case hashes. - Started appearing Jan 2021
                new Regex(@"^[0-9a-zA-Z]{39}", RegexOptions.Compiled),

                // additional Generic match for mixed-case hashes. - Started appearing Jan 2021
                new Regex(@"^[0-9a-zA-Z]{24}", RegexOptions.Compiled),
            };

        private static readonly Regex[] SeasonFolderRegexes = new Regex[]
            {
                new Regex(@"^(Season[ ._-]*\d+|Specials)$", RegexOptions.Compiled)
            };

        // Regex to detect whether the title was reversed.
        private static readonly Regex ReversedTitleRegex = new Regex(@"(?:^|[-._ ])(p027|p0801|\d{2,3}E\d{2}S)[-._ ]", RegexOptions.Compiled);

        private static readonly RegexReplace NormalizeRegex = new RegexReplace(@"((?:\b|_)(?<!^)(a(?!$)|an|the|and|or|of)(?!$)(?:\b|_))|\W|_",
                                                                string.Empty,
                                                                RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex PercentRegex = new Regex(@"(?<=\b\d+)%", RegexOptions.Compiled);

        private static readonly Regex FileExtensionRegex = new Regex(@"\.[a-z0-9]{2,4}$",
                                                                RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly RegexReplace SimpleTitleRegex = new RegexReplace(@"(?:(480|540|576|720|1080|2160)[ip]|[xh][\W_]?26[45]|DD\W?5\W1|[<>?*]|848x480|1280x720|1920x1080|3840x2160|4096x2160|(8|10)b(it)?|10-bit)\s*?",
                                                                string.Empty,
                                                                RegexOptions.IgnoreCase | RegexOptions.Compiled);

        // Valid TLDs http://data.iana.org/TLD/tlds-alpha-by-domain.txt

        private static readonly RegexReplace WebsitePrefixRegex = new RegexReplace(@"^(?:\[\s*)?(?:www\.)?[-a-z0-9-]{1,256}\.(?:[a-z]{2,6}\.[a-z]{2,6}|xn--[a-z0-9-]{4,}|[a-z]{2,})\b(?:\s*\]|[ -]{2,})[ -]*",
                                                                string.Empty,
                                                                RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly RegexReplace WebsitePostfixRegex = new RegexReplace(@"(?:\[\s*)?(?:www\.)?[-a-z0-9-]{1,256}\.(?:xn--[a-z0-9-]{4,}|[a-z]{2,6})\b(?:\s*\])$",
                                                                string.Empty,
                                                                RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex SixDigitAirDateRegex = new Regex(@"(?<=[_.-])(?<airdate>(?<!\d)(?<airyear>[1-9]\d{1})(?<airmonth>[0-1][0-9])(?<airday>[0-3][0-9]))(?=[_.-])",
                                                                        RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly RegexReplace CleanReleaseGroupRegex = new RegexReplace(@"^(.*?[-._ ](S\d+E\d+)[-._ ])|(-(RP|1|NZBGeek|Obfuscated|Scrambled|sample|Pre|postbot|xpost|Rakuv[a-z0-9]*|WhiteRev|BUYMORE|AsRequested|AlternativeToRequested|GEROV|Z0iDS3N|Chamele0n|4P|4Planet|AlteZachen|RePACKPOST))+$",
                                                                string.Empty,
                                                                RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly RegexReplace CleanTorrentSuffixRegex = new RegexReplace(@"\[(?:ettv|rartv|rarbg|cttv|publichd)\]$",
                                                                string.Empty,
                                                                RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex CleanQualityBracketsRegex = new Regex(@"\[[a-z0-9 ._-]+\]$",
                                                                   RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex ReleaseGroupRegex = new Regex(@"-(?<releasegroup>[a-z0-9]+(?<part2>-[a-z0-9]+)?(?!.+?(?:480p|576p|720p|1080p|2160p)))(?<!(?:WEB-DL|Blu-Ray|480p|576p|720p|1080p|2160p|DTS-HD|DTS-X|DTS-MA|DTS-ES|-ES|-EN|-CAT|[ ._]\d{4}-\d{2}|-\d{2})(?:\k<part2>)?)(?:\b|[-._ ]|$)|[-._ ]\[(?<releasegroup>[a-z0-9]+)\]$",
                                                                RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex InvalidReleaseGroupRegex = new Regex(@"^([se]\d+|[0-9a-f]{8})$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex AnimeReleaseGroupRegex = new Regex(@"^(?:\[(?<subgroup>(?!\s).+?(?<!\s))\](?:_|-|\s|\.)?)",
                                                                RegexOptions.IgnoreCase | RegexOptions.Compiled);

        // Handle Exception Release Groups that don't follow -RlsGrp; Manual List
        // name only...be very careful with this last; high chance of false positives
        private static readonly Regex ExceptionReleaseGroupRegexExact = new Regex(@"(?<releasegroup>(?:D\-Z0N3|Fight-BB|VARYG)\b)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        // groups whose releases end with RlsGroup) or RlsGroup]
        private static readonly Regex ExceptionReleaseGroupRegex = new Regex(@"(?<releasegroup>(Silence|afm72|Panda|Ghost|MONOLITH|Tigole|Joy|ImE|UTR|t3nzin|Anime Time|Project Angel|Hakata Ramen|HONE|Vyndros|SEV|Garshasp|Kappa|Natty|RCVR|SAMPA|YOGI|r00t|EDGE2020)(?=\]|\)))", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex YearInTitleRegex = new Regex(@"^(?<title>.+?)[-_. ]+?[\(\[]?(?<year>\d{4})[\]\)]?",
                                                                RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex TitleComponentsRegex = new Regex(@"^(?:(?<title>.+?) \((?<title>.+?)\)|(?<title>.+?) \| (?<title>.+?))$",
                                                                RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex PunctuationRegex = new Regex(@"[^\w\s]", RegexOptions.Compiled);
        private static readonly Regex ArticleWordRegex = new Regex(@"^(a|an|the)\s", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex SpecialEpisodeWordRegex = new Regex(@"\b(part|special|edition|christmas)\b\s?", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex DuplicateSpacesRegex = new Regex(@"\s{2,}", RegexOptions.Compiled);

        private static readonly Regex RequestInfoRegex = new Regex(@"^(?:\[.+?\])+", RegexOptions.Compiled);

        private static readonly string[] Numbers = new[] { "zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine" };

        public static ParsedEpisodeInfo ParsePath(string path)
        {
            var fileInfo = new FileInfo(path);

            var result = ParseTitle(fileInfo.Name);

            if (result == null)
            {
                Logger.Debug("Attempting to parse episode info using combined directory and file names. {0}", fileInfo.Directory.Name);
                result = ParseTitle(fileInfo.Directory.Name + " " + fileInfo.Name);
            }

            if (result == null)
            {
                Logger.Debug("Attempting to parse episode info using directory name. {0}", fileInfo.Directory.Name);
                result = ParseTitle(fileInfo.Directory.Name + fileInfo.Extension);
            }

            return result;
        }

        public static string SimplifyTitle(string title)
        {
            if (!ValidateBeforeParsing(title))
            {
                return title;
            }

            Logger.Debug("Parsing string '{0}'", title);

            if (ReversedTitleRegex.IsMatch(title))
            {
                var titleWithoutExtension = RemoveFileExtension(title).ToCharArray();
                Array.Reverse(titleWithoutExtension);

                title = string.Concat(new string(titleWithoutExtension), title.AsSpan(titleWithoutExtension.Length));

                Logger.Debug("Reversed name detected. Converted to '{0}'", title);
            }

            var simpleTitle = title;

            simpleTitle = WebsitePrefixRegex.Replace(simpleTitle);
            simpleTitle = WebsitePostfixRegex.Replace(simpleTitle);

            simpleTitle = CleanTorrentSuffixRegex.Replace(simpleTitle);

            return simpleTitle;
        }

        public static ParsedEpisodeInfo ParseTitle(string title)
        {
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

                    title = string.Concat(new string(titleWithoutExtension), title.AsSpan(titleWithoutExtension.Length));

                    Logger.Debug("Reversed name detected. Converted to '{0}'", title);
                }

                var releaseTitle = RemoveFileExtension(title);

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

                // TODO: Quick fix stripping [url] - prefixes and postfixes.
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

                var sixDigitAirDateMatch = SixDigitAirDateRegex.Match(simpleTitle);
                if (sixDigitAirDateMatch.Success)
                {
                    var airYear = sixDigitAirDateMatch.Groups["airyear"].Value;
                    var airMonth = sixDigitAirDateMatch.Groups["airmonth"].Value;
                    var airDay = sixDigitAirDateMatch.Groups["airday"].Value;

                    if (airMonth != "00" || airDay != "00")
                    {
                        var fixedDate = string.Format("20{0}.{1}.{2}", airYear, airMonth, airDay);

                        simpleTitle = simpleTitle.Replace(sixDigitAirDateMatch.Groups["airdate"].Value, fixedDate);
                    }
                }

                foreach (var regex in ReportTitleRegex)
                {
                    var match = regex.Matches(simpleTitle);

                    if (match.Count != 0)
                    {
                        Logger.Trace(regex);
                        try
                        {
                            var result = ParseMatchCollection(match, releaseTitle);

                            if (result != null)
                            {
                                result.Languages = LanguageParser.ParseLanguages(releaseTitle);
                                Logger.Debug("Languages parsed: {0}", string.Join(", ", result.Languages));

                                result.Quality = QualityParser.ParseQuality(title);
                                Logger.Debug("Quality parsed: {0}", result.Quality);

                                result.ReleaseGroup = ParseReleaseGroup(releaseTitle);

                                var subGroup = GetSubGroup(match);
                                if (!subGroup.IsNullOrWhiteSpace())
                                {
                                    result.ReleaseGroup = subGroup;
                                }

                                Logger.Debug("Release Group parsed: {0}", result.ReleaseGroup);

                                result.ReleaseHash = GetReleaseHash(match);
                                if (!result.ReleaseHash.IsNullOrWhiteSpace())
                                {
                                    Logger.Debug("Release Hash parsed: {0}", result.ReleaseHash);
                                }

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

        public static string ParseSeriesName(string title)
        {
            Logger.Debug("Parsing string '{0}'", title);

            var parseResult = ParseTitle(title);

            if (parseResult == null)
            {
                return CleanSeriesTitle(title);
            }

            return parseResult.SeriesTitle;
        }

        public static string CleanSeriesTitle(this string title)
        {
            // If Title only contains numbers return it as is.
            if (long.TryParse(title, out _))
            {
                return title;
            }

            // Replace `%` with `percent` to deal with the 3% case
            title = PercentRegex.Replace(title, "percent");

            return NormalizeRegex.Replace(title).ToLower().RemoveAccent();
        }

        public static string NormalizeEpisodeTitle(string title)
        {
            var match = SpecialEpisodeTitleRegex
                        .Select(v => v.Match(title))
                        .FirstOrDefault(v => v.Success);

            if (match != null)
            {
                title = match.Groups["episodetitle"].Value;
            }

            // Disabled, Until we run into specific testcases for the removal of these words.
            // title = SpecialEpisodeWordRegex.Replace(title, string.Empty);

            title = PunctuationRegex.Replace(title, " ");
            title = DuplicateSpacesRegex.Replace(title, " ");

            return title.Trim()
                        .ToLower();
        }

        public static string NormalizeTitle(string title)
        {
            title = PunctuationRegex.Replace(title, string.Empty);
            title = ArticleWordRegex.Replace(title, string.Empty);
            title = DuplicateSpacesRegex.Replace(title, " ");

            return title.Trim().ToLower();
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

        public static string ParseReleaseGroup(string title)
        {
            title = title.Trim();
            title = RemoveFileExtension(title);
            foreach (var replace in PreSubstitutionRegex)
            {
                if (replace.TryReplace(ref title))
                {
                    break;
                }
            }

            title = WebsitePrefixRegex.Replace(title);
            title = CleanTorrentSuffixRegex.Replace(title);

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

        private static SeriesTitleInfo GetSeriesTitleInfo(string title)
        {
            var seriesTitleInfo = new SeriesTitleInfo();
            seriesTitleInfo.Title = title;

            var match = YearInTitleRegex.Match(title);

            if (!match.Success)
            {
                seriesTitleInfo.TitleWithoutYear = title;
            }
            else
            {
                seriesTitleInfo.TitleWithoutYear = match.Groups["title"].Value;
                seriesTitleInfo.Year = Convert.ToInt32(match.Groups["year"].Value);
            }

            var matchComponents = TitleComponentsRegex.Match(seriesTitleInfo.TitleWithoutYear);

            if (matchComponents.Success)
            {
                seriesTitleInfo.AllTitles = matchComponents.Groups["title"].Captures.OfType<Capture>().Select(v => v.Value).ToArray();
            }

            return seriesTitleInfo;
        }

        private static ParsedEpisodeInfo ParseMatchCollection(MatchCollection matchCollection, string releaseTitle)
        {
            var seriesName = matchCollection[0].Groups["title"].Value.Replace('.', ' ').Replace('_', ' ');
            seriesName = RequestInfoRegex.Replace(seriesName, "").Trim(' ');

            int.TryParse(matchCollection[0].Groups["airyear"].Value, out var airYear);

            var lastSeasonEpisodeStringIndex = matchCollection[0].Groups["title"].EndIndex();

            ParsedEpisodeInfo result;

            if (!matchCollection[0].Groups["airyear"].Success)
            {
                result = new ParsedEpisodeInfo
                {
                    ReleaseTitle = releaseTitle
                };

                foreach (Match matchGroup in matchCollection)
                {
                    var episodeCaptures = matchGroup.Groups["episode"].Captures.Cast<Capture>().ToList();

                    // Allows use to return a list of 0 episodes (We can handle that as a full season release)
                    if (episodeCaptures.Any())
                    {
                        var first = ParseNumber(episodeCaptures.First().Value);
                        var last = ParseNumber(episodeCaptures.Last().Value);

                        if (first > last)
                        {
                            return null;
                        }

                        var count = last - first + 1;

                        lastSeasonEpisodeStringIndex = Math.Max(lastSeasonEpisodeStringIndex, episodeCaptures.Last().EndIndex());
                    }
                }

                var seasons = new List<int>();

                foreach (Capture seasonCapture in matchCollection[0].Groups["season"].Captures)
                {
                    if (int.TryParse(seasonCapture.Value, out var parsedSeason))
                    {
                        seasons.Add(parsedSeason);

                        lastSeasonEpisodeStringIndex = Math.Max(lastSeasonEpisodeStringIndex, seasonCapture.EndIndex());
                    }
                }
            }
            else
            {
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

                lastSeasonEpisodeStringIndex = Math.Max(lastSeasonEpisodeStringIndex, matchCollection[0].Groups["airyear"].EndIndex());
                lastSeasonEpisodeStringIndex = Math.Max(lastSeasonEpisodeStringIndex, matchCollection[0].Groups["airmonth"].EndIndex());
                lastSeasonEpisodeStringIndex = Math.Max(lastSeasonEpisodeStringIndex, matchCollection[0].Groups["airday"].EndIndex());

                result = new ParsedEpisodeInfo
                {
                    ReleaseTitle = releaseTitle,
                    AirDate = airDate.ToString(Episode.AIR_DATE_FORMAT),
                };
            }

            if (matchCollection[0].Groups["releasetoken"].Success)
            {
                result.ReleaseTokens = matchCollection[0].Groups["releasetoken"].Value;
            }

            if (result.ReleaseTokens.IsNullOrWhiteSpace())
            {
                if (lastSeasonEpisodeStringIndex != releaseTitle.Length)
                {
                    result.ReleaseTokens = releaseTitle.Substring(lastSeasonEpisodeStringIndex);
                }
                else
                {
                    result.ReleaseTokens = releaseTitle;
                }
            }

            result.SeriesTitle = seriesName;
            result.SeriesTitleInfo = GetSeriesTitleInfo(result.SeriesTitle);

            Logger.Debug("Episode Parsed. {0}", result);

            return result;
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

            if (RejectHashedReleasesRegexes.Any(v => v.IsMatch(titleWithoutExtension)))
            {
                Logger.Debug("Rejected Hashed Release Title: " + title);
                return false;
            }

            if (SeasonFolderRegexes.Any(v => v.IsMatch(titleWithoutExtension)))
            {
                Logger.Debug("Rejected Season Folder Release Title: " + title);
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

        private static int ParseNumber(string value)
        {
            var normalized = value.Normalize(NormalizationForm.FormKC);

            if (int.TryParse(normalized, out var number))
            {
                return number;
            }

            number = Array.IndexOf(Numbers, value.ToLower());

            if (number != -1)
            {
                return number;
            }

            throw new FormatException(string.Format("{0} isn't a number", value));
        }

        private static decimal ParseDecimal(string value)
        {
            var normalized = value.Normalize(NormalizationForm.FormKC);

            if (decimal.TryParse(normalized, NumberStyles.Float, CultureInfo.InvariantCulture, out var number))
            {
                return number;
            }

            throw new FormatException(string.Format("{0} isn't a number", value));
        }
    }
}
