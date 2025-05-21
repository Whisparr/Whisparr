using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Diacritical;
using NLog;
using NzbDrone.Common.EnsureThat;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.MediaInfo;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Movies.Performers;
using NzbDrone.Core.Movies.Studios;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.Organizer
{
    public interface IBuildFileNames
    {
        string BuildFileName(Movie movie, MovieFile movieFile, NamingConfig namingConfig = null, List<CustomFormat> customFormats = null, bool sample = false);
        string BuildFilePath(Movie movie, string fileName, string extension);
        string BuildFilePath(string path, string fileName, string extension);
        string GetMovieFolder(Movie movie, NamingConfig namingConfig = null);
        string CleanTitle(string title);
    }

    public class FileNameBuilder : IBuildFileNames
    {
        private const string MediaInfoVideoDynamicRangeToken = "{MediaInfo VideoDynamicRange}";
        private const string MediaInfoVideoDynamicRangeTypeToken = "{MediaInfo VideoDynamicRangeType}";

        private readonly INamingConfigService _namingConfigService;
        private readonly IQualityDefinitionService _qualityDefinitionService;
        private readonly IStudioService _studioService;
        private readonly IUpdateMediaInfo _mediaInfoUpdater;
        private readonly ICustomFormatCalculationService _formatCalculator;
        private readonly Logger _logger;

        private int _trimEnd;

        private static readonly Regex TitleRegex = new Regex(@"(?<tag>\{(?:imdb-|edition-))?\{(?<prefix>[- ._\[(]*)(?<token>(?:[a-z0-9]+)(?:(?<separator>[- ._]+)(?:[a-z0-9]+))?)(?::(?<customFormat>[ ,a-z0-9|+-]+(?<![- ])))?(?<suffix>[-} ._)\]]*)\}",
                                                             RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        public static readonly Regex MovieTitleRegex = new Regex(@"(?<token>\{((?:(Movie|Original))(?<separator>[- ._])(Clean)?(Original)?(Title|Filename)(The)?)(?::(?<customFormat>[a-z0-9|-]+))?\})",
                                                                            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static readonly Regex SceneFolderRegex = new Regex(@"(?<token>\{((?:(Studio|Original))(?<separator>[- ._])(Clean)?(Original)?(Title|Filename)(The)?)(?::(?<customFormat>[a-z0-9|]+))?\})",
                                                                            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static readonly Regex MainFolderRegex = new Regex(@"^(?<main>(?:[a-zA-Z0-9]+(?:\\|\/)))",
                                                                            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static readonly Regex SceneTitleRegex = new Regex(@"(?<token>\{((?:(Scene|Original))(?<separator>[- ._])(Clean)?(Original)?(Title|Filename)(The)?)(?::(?<customFormat>[a-z0-9|]+))?\})",
                                                                            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex FileNameCleanupRegex = new Regex(@"([- ._])(\1)+", RegexOptions.Compiled);
        private static readonly Regex TrimSeparatorsRegex = new Regex(@"[- ._]+$", RegexOptions.Compiled);

        private static readonly Regex ScenifyRemoveChars = new Regex(@"(?<=\s)(,|<|>|\/|\\|;|:|'|""|\||`|’|~|!|\?|@|$|%|^|\*|-|_|=){1}(?=\s)|('|`|’|:|\?|,)(?=(?:(?:s|m|t|ve|ll|d|re)\s)|\s|$)|(\(|\)|\[|\]|\{|\})", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex ScenifyReplaceChars = new Regex(@"[\/]", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex EmojiRegex = new Regex(@"\p{Cs}", RegexOptions.Compiled);
        private static readonly Regex WordDelimiterRegex = new Regex(@"(’|')+", RegexOptions.Compiled);

        private static readonly Regex TitlePrefixRegex = new Regex(@"^(The|An|A) (.*?)((?: *\([^)]+\))*)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex ReservedDeviceNamesRegex = new Regex(@"^(?:aux|com[1-9]|con|lpt[1-9]|nul|prn)\.", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        // generated from https://www.loc.gov/standards/iso639-2/ISO-639-2_utf-8.txt
        public static readonly ImmutableDictionary<string, string> Iso639BTMap = new Dictionary<string, string>
        {
            { "alb", "sqi" },
            { "arm", "hye" },
            { "baq", "eus" },
            { "bur", "mya" },
            { "chi", "zho" },
            { "cze", "ces" },
            { "dut", "nld" },
            { "fre", "fra" },
            { "geo", "kat" },
            { "ger", "deu" },
            { "gre", "ell" },
            { "gsw", "deu" },
            { "ice", "isl" },
            { "mac", "mkd" },
            { "mao", "mri" },
            { "may", "msa" },
            { "per", "fas" },
            { "rum", "ron" },
            { "slo", "slk" },
            { "tib", "bod" },
            { "wel", "cym" },
            { "khk", "mon" },
            { "mvf", "mon" }
        }.ToImmutableDictionary();

        public static readonly ImmutableArray<string> BadCharacters = ImmutableArray.Create("\\", "/", "<", ">", "?", "*", "|", "\"");
        public static readonly ImmutableArray<string> GoodCharacters = ImmutableArray.Create("+", "+", "", "", "!", "-", "", "");

        public FileNameBuilder(INamingConfigService namingConfigService,
                               IQualityDefinitionService qualityDefinitionService,
                               IStudioService studioService,
                               IUpdateMediaInfo mediaInfoUpdater,
                               ICustomFormatCalculationService formatCalculator,
                               Logger logger)
        {
            _namingConfigService = namingConfigService;
            _qualityDefinitionService = qualityDefinitionService;
            _studioService = studioService;
            _mediaInfoUpdater = mediaInfoUpdater;
            _formatCalculator = formatCalculator;
            _logger = logger;
            _trimEnd = 0;
        }

        public string BuildFileName(Movie movie, MovieFile movieFile, NamingConfig namingConfig = null, List<CustomFormat> customFormats = null, bool sample = false)
        {
            if (namingConfig == null)
            {
                namingConfig = _namingConfigService.GetConfig();
            }

            var itemType = movie.MovieMetadata.Value.ItemType;

            if ((itemType == ItemType.Movie && !namingConfig.RenameMovies) || (itemType == ItemType.Scene && !namingConfig.RenameScenes))
            {
                if (!sample)
                {
                    return GetOriginalTitle(movieFile, false);
                }
            }

            if (namingConfig.StandardMovieFormat.IsNullOrWhiteSpace())
            {
                throw new NamingFormatException("Standard movie format cannot be empty");
            }

            var pattern = itemType == ItemType.Movie ? namingConfig.StandardMovieFormat : namingConfig.StandardSceneFormat;
            var tokenHandlers = new Dictionary<string, Func<TokenMatch, string>>(FileNameBuilderTokenEqualityComparer.Instance);
            var multipleTokens = TitleRegex.Matches(pattern).Count > 1;

            UpdateMediaInfoIfNeeded(pattern, movieFile, movie);

            if (itemType == ItemType.Movie)
            {
                AddMovieTokens(tokenHandlers, movie);
            }
            else
            {
                AddStudioTokens(tokenHandlers, movie);
                AddSceneTokens(tokenHandlers, movie);
                AddSceneTitlePlaceholderTokens(tokenHandlers, movie);
            }

            AddReleaseDateTokens(tokenHandlers, movie.Year);
            AddIdTokens(tokenHandlers, movie);
            AddQualityTokens(tokenHandlers, movie, movieFile);
            AddMediaInfoTokens(tokenHandlers, movieFile);
            AddMovieFileTokens(tokenHandlers, movieFile, multipleTokens);
            AddEditionTagsTokens(tokenHandlers, movieFile);
            AddCustomFormats(tokenHandlers, movie, movieFile, customFormats);

            var splitPatterns = pattern.Split(new char[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries);
            var components = new List<string>();

            foreach (var s in splitPatterns)
            {
                var splitPattern = s;

                var component = ReplaceTokens(splitPattern, tokenHandlers, namingConfig).Trim();
                var fileLengthLimit = splitPatterns.Last() == s ? namingConfig.MaxFilePathLength : namingConfig.MaxFolderPathLength;
                if (fileLengthLimit > 0)
                {
                    while (component.Length > fileLengthLimit && _trimEnd < fileLengthLimit)
                    {
                        _trimEnd++;
                        component = ReplaceTokens(splitPattern, tokenHandlers, namingConfig).Trim();
                    }
                }

                component = FileNameCleanupRegex.Replace(component, match => match.Captures[0].Value[0].ToString());
                component = TrimSeparatorsRegex.Replace(component, string.Empty);
                component = component.Replace("{ellipsis}", "...");
                component = ReplaceReservedDeviceNames(component);

                if (component.IsNotNullOrWhiteSpace())
                {
                    components.Add(component);
                }
            }

            return Path.Combine(components.ToArray());
        }

        public string BuildFilePath(Movie movie, string fileName, string extension)
        {
            Ensure.That(extension, () => extension).IsNotNullOrWhiteSpace();

            var path = movie.Path;

            return Path.Combine(path, fileName + extension);
        }

        public string BuildFilePath(string path, string fileName, string extension)
        {
            Ensure.That(extension, () => extension).IsNotNullOrWhiteSpace();

            return Path.Combine(path, fileName + extension);
        }

        public string GetMovieFolder(Movie movie, NamingConfig namingConfig = null)
        {
            if (namingConfig == null)
            {
                namingConfig = _namingConfigService.GetConfig();
            }

            var itemType = movie.MovieMetadata.Value.ItemType;

            var pattern = itemType == ItemType.Movie ? namingConfig.MovieFolderFormat : namingConfig.SceneFolderFormat;
            var tokenHandlers = new Dictionary<string, Func<TokenMatch, string>>(FileNameBuilderTokenEqualityComparer.Instance);
            var multipleTokens = TitleRegex.Matches(pattern).Count > 1;

            if (itemType == ItemType.Movie)
            {
                AddMovieTokens(tokenHandlers, movie);
            }
            else
            {
                AddSceneTokens(tokenHandlers, movie);
                AddSceneTitlePlaceholderTokens(tokenHandlers, movie);
            }

            AddStudioTokens(tokenHandlers, movie);
            AddReleaseDateTokens(tokenHandlers, movie.Year);
            AddIdTokens(tokenHandlers, movie);

            var movieFile = movie.MovieFile;

            if (movie.MovieFile != null)
            {
                AddQualityTokens(tokenHandlers, movie, movieFile);
                AddMediaInfoTokens(tokenHandlers, movieFile);
                AddMovieFileTokens(tokenHandlers, movieFile, multipleTokens);
                AddEditionTagsTokens(tokenHandlers, movieFile);
            }
            else
            {
                AddMovieFileTokens(tokenHandlers, new MovieFile { SceneName = $"{movie.Title} {movie.Year}", RelativePath = $"{movie.Title} {movie.Year}" }, multipleTokens);
            }

            var splitPatterns = pattern.Split(new char[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries);
            var components = new List<string>();

            foreach (var s in splitPatterns)
            {
                var splitPattern = s;

                var component = ReplaceTokens(splitPattern, tokenHandlers, namingConfig);
                component = CleanFolderName(component);
                component = component.Replace("{ellipsis}", "...");
                component = ReplaceReservedDeviceNames(component);

                var folderLengthLimit = namingConfig.MaxFolderPathLength;
                if (folderLengthLimit > 0 && component.Length > folderLengthLimit)
                {
                    component = component.Substring(0, folderLengthLimit);
                }

                if (component.IsNotNullOrWhiteSpace())
                {
                    components.Add(component);
                }
            }

            return Path.Combine(components.ToArray());
        }

        public string CleanTitle(string title)
        {
            if (title.IsNullOrWhiteSpace())
            {
                return string.Empty;
            }

            title = title.Replace("&", "and");
            title = ScenifyReplaceChars.Replace(title, " ");
            title = EmojiRegex.Replace(title, " ");
            title = ScenifyRemoveChars.Replace(title, string.Empty);

            return title.RemoveDiacritics();
        }

        public static string TitleThe(string title)
        {
            return TitlePrefixRegex.Replace(title, "$2, $1$3");
        }

        public static string TitleFirstCharacter(string title)
        {
            if (char.IsLetterOrDigit(title[0]))
            {
                return title.Substring(0, 1).ToUpper().RemoveDiacritics()[0].ToString();
            }

            // Try the second character if the first was non alphanumeric
            if (char.IsLetterOrDigit(title[1]))
            {
                return title.Substring(1, 1).ToUpper().RemoveDiacritics()[0].ToString();
            }

            // Default to "_" if no alphanumeric character can be found in the first 2 positions
            return "_";
        }

        public static string CleanFileName(string name)
        {
            return CleanFileName(name, NamingConfig.Default);
        }

        public static string CleanFolderName(string name)
        {
            name = FileNameCleanupRegex.Replace(name, match => match.Captures[0].Value[0].ToString());

            return name.Trim(' ', '.');
        }

        public static string SlugTitle(string title)
        {
            title = title.Replace(" ", "");
            title = ScenifyRemoveChars.Replace(title, string.Empty);
            title = ScenifyReplaceChars.Replace(title, " ");

            return title;
        }

        private void AddMovieTokens(Dictionary<string, Func<TokenMatch, string>> tokenHandlers, Movie movie)
        {
            tokenHandlers["{Movie Title}"] = m => Truncate(GetLanguageTitle(movie, m.CustomFormat), m.CustomFormat);
            tokenHandlers["{Movie CleanTitle}"] = m => Truncate(CleanTitle(GetLanguageTitle(movie, m.CustomFormat)), m.CustomFormat);
            tokenHandlers["{Movie TitleThe}"] = m => Truncate(TitleThe(movie.Title), m.CustomFormat);
            tokenHandlers["{Movie TitleFirstCharacter}"] = m => TitleFirstCharacter(TitleThe(GetLanguageTitle(movie, m.CustomFormat)));
        }

        private void AddStudioTokens(Dictionary<string, Func<TokenMatch, string>> tokenHandlers, Movie movie)
        {
            if (movie.MovieMetadata.Value.StudioTitle.IsNotNullOrWhiteSpace())
            {
                tokenHandlers["{Studio Title}"] = m => movie.MovieMetadata.Value.StudioTitle;
                tokenHandlers["{Studio TitleSlug}"] = m => SlugTitle(movie.MovieMetadata.Value.StudioTitle);
                tokenHandlers["{Studio CleanTitle}"] = m => CleanTitle(movie.MovieMetadata.Value.StudioTitle);
                tokenHandlers["{Studio CleanTitleSlug}"] = m => SlugTitle(CleanTitle(movie.MovieMetadata.Value.StudioTitle));
                tokenHandlers["{Studio TitleThe}"] = m => TitleThe(movie.MovieMetadata.Value.StudioTitle);
                tokenHandlers["{Studio TitleFirstCharacter}"] = m => TitleThe(movie.MovieMetadata.Value.StudioTitle).Substring(0, 1).FirstCharToUpper();
            }

            if (movie.MovieMetadata.Value.Studio != null && movie.MovieMetadata.Value.Studio.Network.IsNotNullOrWhiteSpace())
            {
                tokenHandlers["{Studio Network}"] = m => movie.MovieMetadata.Value.Studio.Network;
                tokenHandlers["{Studio CleanNetwork}"] = m => CleanTitle(movie.MovieMetadata.Value.Studio.Network);
                tokenHandlers["{Studio CleanNetworkSlug}"] = m => SlugTitle(CleanTitle(movie.MovieMetadata.Value.Studio.Network));
            }
            else if (movie.MovieMetadata.Value.StudioForeignId.IsNotNullOrWhiteSpace())
            {
                var studio = _studioService.FindByForeignId(movie.MovieMetadata.Value.StudioForeignId);
                tokenHandlers["{Studio Network}"] = m => studio?.Network ?? string.Empty;
                tokenHandlers["{Studio CleanNetwork}"] = m => CleanTitle(studio?.Network ?? string.Empty);
                tokenHandlers["{Studio CleanNetworkSlug}"] = m => SlugTitle(CleanTitle(studio?.Network ?? string.Empty));
            }
            else
            {
                tokenHandlers["{Studio Network}"] = m => string.Empty;
                tokenHandlers["{Studio CleanNetwork}"] = m => string.Empty;
                tokenHandlers["{Studio CleanNetworkSlug}"] = m => string.Empty;
            }
        }

        private void AddSceneTokens(Dictionary<string, Func<TokenMatch, string>> tokenHandlers, Movie movie)
        {
            if (movie.MovieMetadata.Value.ReleaseDate.IsNotNullOrWhiteSpace())
            {
                var releaseDate = movie.MovieMetadata.Value.ReleaseDate;
                tokenHandlers["{Release Date}"] = m => releaseDate;
                tokenHandlers["{Release ShortDate}"] = m => releaseDate.Replace("-", " ").Substring(2);
            }
            else
            {
                tokenHandlers["{Release Date}"] = m => "Unknown";
            }

            if (movie.MovieMetadata.Value.Credits != null)
            {
                var credits = movie.MovieMetadata.Value.Credits;
                tokenHandlers["{Scene Performers}"] = m => credits.OrderBy(p => p.Performer.Name)
                    .Select(p => p.Performer.Name)
                    .Take(4)
                    .Join(" ");
                tokenHandlers["{Scene PerformersFemale}"] = m => credits.Where(p => p.Performer.Gender == Gender.Female)
                    .OrderBy(p => p.Performer.Name)
                    .Select(p => p.Performer.Name)
                    .Take(4)
                    .Join(" ");
                tokenHandlers["{Scene PerformersMale}"] = m => credits.Where(p => p.Performer.Gender == Gender.Male)
                    .OrderBy(p => p.Performer.Name)
                    .Select(p => p.Performer.Name)
                    .Take(4)
                    .Join(" ");
                var performersOther = new[] { Gender.TransMale, Gender.TransFemale, Gender.NonBinary, Gender.Intersex };
                tokenHandlers["{Scene PerformersOther}"] = m => credits.Where(p => performersOther.Contains(p.Performer.Gender))
                    .OrderBy(p => p.Performer.Name)
                    .Select(p => p.Performer.Name)
                    .Take(4)
                    .Join(" ");
                tokenHandlers["{Scene PerformersFemaleAlias}"] = m => credits.Where(p => p.Performer.Gender == Gender.Female)
                    .OrderBy(p => p.Performer.Name)
                    .Select(p => !string.IsNullOrWhiteSpace(p.Character) ? p.Character : p.Performer.Name)
                    .Take(4)
                    .Join(" ");
                tokenHandlers["{Scene PerformersMaleAlias}"] = m => credits.Where(p => p.Performer.Gender == Gender.Male)
                    .OrderBy(p => p.Performer.Name)
                    .Select(p => !string.IsNullOrWhiteSpace(p.Character) ? p.Character : p.Performer.Name)
                    .Take(4)
                    .Join(" ");
                tokenHandlers["{Scene PerformersOtherAlias}"] = m => credits.Where(p => performersOther.Contains(p.Performer.Gender))
                    .OrderBy(p => p.Performer.Name)
                    .Select(p => !string.IsNullOrWhiteSpace(p.Character) ? p.Character : p.Performer.Name)
                    .Take(4)
                    .Join(" ");
                tokenHandlers["{Scene PerformersAlias}"] = m => credits.OrderBy(p => p.Performer.Name)
                    .Select(p => !string.IsNullOrWhiteSpace(p.Character) ? p.Character : p.Performer.Name)
                    .Take(4)
                    .Join(" ");

                // Clean versions
                tokenHandlers["{Scene CleanPerformers}"] = m => credits.OrderBy(p => p.Performer.Name)
                    .Select(p => p.Performer.Name.CleanPerformer())
                    .Take(4)
                    .Join(" ");
                tokenHandlers["{Scene CleanPerformersFemale}"] = m => credits.Where(p => p.Performer.Gender == Gender.Female)
                    .OrderBy(p => p.Performer.Name)
                    .Select(p => p.Performer.Name.CleanPerformer())
                    .Take(4)
                    .Join(" ");
                tokenHandlers["{Scene CleanPerformersMale}"] = m => credits.Where(p => p.Performer.Gender == Gender.Male)
                    .OrderBy(p => p.Performer.Name)
                    .Select(p => p.Performer.Name.CleanPerformer())
                    .Take(4)
                    .Join(" ");
                tokenHandlers["{Scene CleanPerformersOther}"] = m => credits.Where(p => performersOther.Contains(p.Performer.Gender))
                    .OrderBy(p => p.Performer.Name)
                    .Select(p => p.Performer.Name.CleanPerformer())
                    .Take(4)
                    .Join(" ");
                tokenHandlers["{Scene CleanPerformersFemaleAlias}"] = m => credits.Where(p => p.Performer.Gender == Gender.Female)
                    .OrderBy(p => p.Performer.Name)
                    .Select(p => !string.IsNullOrWhiteSpace(p.Character) ? p.Character.CleanPerformer() : p.Performer.Name.CleanPerformer())
                    .Take(4)
                    .Join(" ");
                tokenHandlers["{Scene CleanPerformersMaleAlias}"] = m => credits.Where(p => p.Performer.Gender == Gender.Male)
                    .OrderBy(p => p.Performer.Name)
                    .Select(p => !string.IsNullOrWhiteSpace(p.Character) ? p.Character.CleanPerformer() : p.Performer.Name.CleanPerformer())
                    .Take(4)
                    .Join(" ");
                tokenHandlers["{Scene CleanPerformersOtherAlias}"] = m => credits.Where(p => performersOther.Contains(p.Performer.Gender))
                    .OrderBy(p => p.Performer.Name)
                    .Select(p => !string.IsNullOrWhiteSpace(p.Character) ? p.Character.CleanPerformer() : p.Performer.Name.CleanPerformer())
                    .Take(4)
                    .Join(" ");
                tokenHandlers["{Scene CleanPerformersAlias}"] = m => credits.OrderBy(p => p.Performer.Name)
                    .Select(p => !string.IsNullOrWhiteSpace(p.Character) ? p.Character.CleanPerformer() : p.Performer.Name.CleanPerformer())
                    .Take(4)
                    .Join(" ");
            }

            if (!string.IsNullOrWhiteSpace(movie.MovieMetadata.Value.Code))
            {
                var code = movie.MovieMetadata.Value.Code;
                tokenHandlers["{Scene Code}"] = m => code;
            }
            else
            {
                tokenHandlers["{Scene Code}"] = m => string.Empty;
            }
        }

        private void AddSceneTitlePlaceholderTokens(Dictionary<string, Func<TokenMatch, string>> tokenHandlers, Movie movie)
        {
            tokenHandlers["{Scene Title}"] = m => GetLanguageTitle(movie, m.CustomFormat).Remove(GetLanguageTitle(movie, m.CustomFormat).Length - _trimEnd, _trimEnd);
            tokenHandlers["{Scene CleanTitle}"] = m => CleanTitle(GetLanguageTitle(movie, m.CustomFormat)).Remove(CleanTitle(GetLanguageTitle(movie, m.CustomFormat)).Length - _trimEnd, _trimEnd);
            tokenHandlers["{Scene TitleThe}"] = m => TitleThe(movie.Title).Remove(TitleThe(movie.Title).Length - _trimEnd, _trimEnd);
            tokenHandlers["{Scene TitleFirstCharacter}"] = m => TitleFirstCharacter(TitleThe(GetLanguageTitle(movie, m.CustomFormat))).Remove(TitleFirstCharacter(TitleThe(GetLanguageTitle(movie, m.CustomFormat))).Length - _trimEnd, _trimEnd);
        }

        private void AddSceneTitleTokens(Dictionary<string, Func<TokenMatch, string>> tokenHandlers, Movie movie, int maxLength)
        {
            tokenHandlers["{Scene Title}"] = m => GetLanguageTitle(movie, m.CustomFormat);
            tokenHandlers["{Scene CleanTitle}"] = m => CleanTitle(GetLanguageTitle(movie, m.CustomFormat));
        }

        private string GetLanguageTitle(Movie movie, string isoCodes)
        {
            return movie.Title;
        }

        private void AddEditionTagsTokens(Dictionary<string, Func<TokenMatch, string>> tokenHandlers, MovieFile movieFile)
        {
            if (movieFile.Edition.IsNotNullOrWhiteSpace())
            {
                tokenHandlers["{Edition Tags}"] = m => Truncate(CultureInfo.CurrentCulture.TextInfo.ToTitleCase(movieFile.Edition.ToLower()), m.CustomFormat);
            }
        }

        private void AddReleaseDateTokens(Dictionary<string, Func<TokenMatch, string>> tokenHandlers, int releaseYear)
        {
            if (releaseYear == 0)
            {
                tokenHandlers["{Release Year}"] = m => string.Empty;
                return;
            }

            tokenHandlers["{Release Year}"] = m => string.Format("{0}", releaseYear.ToString()); // Do I need m.CustomFormat?
        }

        private void AddIdTokens(Dictionary<string, Func<TokenMatch, string>> tokenHandlers, Movie movie)
        {
            tokenHandlers["{ImdbId}"] = m => movie.MovieMetadata.Value.ImdbId ?? string.Empty;
            tokenHandlers["{TmdbId}"] = m => movie.MovieMetadata.Value.TmdbId.ToString();
            tokenHandlers["{StashId}"] = m => movie.MovieMetadata.Value.StashId;
        }

        private void AddMovieFileTokens(Dictionary<string, Func<TokenMatch, string>> tokenHandlers, MovieFile movieFile, bool multipleTokens)
        {
            tokenHandlers["{Original Title}"] = m => GetOriginalTitle(movieFile, multipleTokens);
            tokenHandlers["{Original Filename}"] = m => GetOriginalFileName(movieFile, multipleTokens);
            tokenHandlers["{Release Group}"] = m => movieFile.ReleaseGroup.IsNullOrWhiteSpace() ? m.DefaultValue("Whisparr") : Truncate(movieFile.ReleaseGroup, m.CustomFormat);
        }

        private void AddQualityTokens(Dictionary<string, Func<TokenMatch, string>> tokenHandlers, Movie movie, MovieFile movieFile)
        {
            if (movieFile?.Quality?.Quality == null)
            {
                tokenHandlers["{Quality Full}"] = m => "";
                tokenHandlers["{Quality Title}"] = m => "";
                tokenHandlers["{Quality Proper}"] = m => "";
                tokenHandlers["{Quality Real}"] = m => "";
                return;
            }

            var qualityTitle = _qualityDefinitionService.Get(movieFile.Quality.Quality).Title;
            var qualityProper = GetQualityProper(movie, movieFile.Quality);
            var qualityReal = GetQualityReal(movie, movieFile.Quality);

            tokenHandlers["{Quality Full}"] = m => string.Format("{0} {1} {2}", qualityTitle, qualityProper, qualityReal);
            tokenHandlers["{Quality Title}"] = m => qualityTitle;
            tokenHandlers["{Quality Proper}"] = m => qualityProper;
            tokenHandlers["{Quality Real}"] = m => qualityReal;
        }

        private static readonly IReadOnlyDictionary<string, int> MinimumMediaInfoSchemaRevisions =
            new Dictionary<string, int>(FileNameBuilderTokenEqualityComparer.Instance)
        {
            { MediaInfoVideoDynamicRangeToken, 5 },
            { MediaInfoVideoDynamicRangeTypeToken, 13 }
        };

        private void AddMediaInfoTokens(Dictionary<string, Func<TokenMatch, string>> tokenHandlers, MovieFile movieFile)
        {
            if (movieFile.MediaInfo == null)
            {
                _logger.Trace("Media info is unavailable for {0}", movieFile);

                return;
            }

            var sceneName = movieFile.GetSceneOrFileName();

            var videoCodec = MediaInfoFormatter.FormatVideoCodec(movieFile.MediaInfo, sceneName) ?? string.Empty;
            var audioCodec = MediaInfoFormatter.FormatAudioCodec(movieFile.MediaInfo, sceneName) ?? string.Empty;
            var audioChannels = MediaInfoFormatter.FormatAudioChannels(movieFile.MediaInfo);
            var audioLanguages = movieFile.MediaInfo.AudioLanguages ?? new List<string>();
            var subtitles = movieFile.MediaInfo.Subtitles ?? new List<string>();

            var videoBitDepth = movieFile.MediaInfo.VideoBitDepth > 0 ? movieFile.MediaInfo.VideoBitDepth.ToString() : 8.ToString();
            var audioChannelsFormatted = audioChannels > 0 ?
                                audioChannels.ToString("F1", CultureInfo.InvariantCulture) :
                                string.Empty;

            var mediaInfo3D = movieFile.MediaInfo.VideoMultiViewCount > 1 ? "3D" : string.Empty;

            tokenHandlers["{MediaInfo Video}"] = m => videoCodec;
            tokenHandlers["{MediaInfo VideoCodec}"] = m => videoCodec;
            tokenHandlers["{MediaInfo VideoBitDepth}"] = m => videoBitDepth;

            tokenHandlers["{MediaInfo Audio}"] = m => audioCodec;
            tokenHandlers["{MediaInfo AudioCodec}"] = m => audioCodec;
            tokenHandlers["{MediaInfo AudioChannels}"] = m => audioChannelsFormatted;
            tokenHandlers["{MediaInfo AudioLanguages}"] = m => GetLanguagesToken(audioLanguages, m.CustomFormat, true, true);
            tokenHandlers["{MediaInfo AudioLanguagesAll}"] = m => GetLanguagesToken(audioLanguages, m.CustomFormat, false, true);

            tokenHandlers["{MediaInfo SubtitleLanguages}"] = m => GetLanguagesToken(subtitles, m.CustomFormat, false, true);
            tokenHandlers["{MediaInfo SubtitleLanguagesAll}"] = m => GetLanguagesToken(subtitles, m.CustomFormat, false, true);

            tokenHandlers["{MediaInfo 3D}"] = m => mediaInfo3D;

            tokenHandlers["{MediaInfo Simple}"] = m => $"{videoCodec} {audioCodec}";
            tokenHandlers["{MediaInfo Full}"] = m => $"{videoCodec} {audioCodec}{GetLanguagesToken(audioLanguages, m.CustomFormat, true, true)} {GetLanguagesToken(subtitles, m.CustomFormat, false, true)}";

            tokenHandlers[MediaInfoVideoDynamicRangeToken] =
                m => MediaInfoFormatter.FormatVideoDynamicRange(movieFile.MediaInfo);
            tokenHandlers[MediaInfoVideoDynamicRangeTypeToken] =
                m => MediaInfoFormatter.FormatVideoDynamicRangeType(movieFile.MediaInfo);
        }

        private void AddCustomFormats(Dictionary<string, Func<TokenMatch, string>> tokenHandlers, Movie movie, MovieFile movieFile, List<CustomFormat> customFormats = null)
        {
            if (customFormats == null)
            {
                movieFile.Movie = movie;
                customFormats = _formatCalculator.ParseCustomFormat(movieFile, movie);
            }

            tokenHandlers["{Custom Formats}"] = m => GetCustomFormatsToken(customFormats, m.CustomFormat);
            tokenHandlers["{Custom Format}"] = m =>
            {
                if (m.CustomFormat.IsNullOrWhiteSpace())
                {
                    return string.Empty;
                }

                return customFormats.FirstOrDefault(x => x.IncludeCustomFormatWhenRenaming && x.Name == m.CustomFormat)?.ToString() ?? string.Empty;
            };
        }

        private string GetCustomFormatsToken(List<CustomFormat> customFormats, string filter)
        {
            var tokens = customFormats.Where(x => x.IncludeCustomFormatWhenRenaming).ToList();

            var filteredTokens = tokens;

            if (filter.IsNotNullOrWhiteSpace())
            {
                if (filter.StartsWith("-"))
                {
                    var splitFilter = filter.Substring(1).Split(',');
                    filteredTokens = tokens.Where(c => !splitFilter.Contains(c.Name)).ToList();
                }
                else
                {
                    var splitFilter = filter.Split(',');
                    filteredTokens = tokens.Where(c => splitFilter.Contains(c.Name)).ToList();
                }
            }

            return string.Join(" ", filteredTokens);
        }

        private string GetLanguagesToken(List<string> mediaInfoLanguages, string filter, bool skipEnglishOnly, bool quoted)
        {
            var tokens = new List<string>();
            foreach (var item in mediaInfoLanguages)
            {
                if (!string.IsNullOrWhiteSpace(item) && item != "und")
                {
                    tokens.Add(item.Trim());
                }
            }

            for (var i = 0; i < tokens.Count; i++)
            {
                try
                {
                    var token = tokens[i].ToLowerInvariant();
                    if (Iso639BTMap.TryGetValue(token, out var mapped))
                    {
                        token = mapped;
                    }

                    var cultureInfo = new CultureInfo(token);
                    tokens[i] = cultureInfo.TwoLetterISOLanguageName.ToUpper();
                }
                catch
                {
                }
            }

            tokens = tokens.Distinct().ToList();

            var filteredTokens = tokens;

            // Exclude or filter
            if (filter.IsNotNullOrWhiteSpace())
            {
                if (filter.StartsWith("-"))
                {
                    filteredTokens = tokens.Except(filter.Split('-')).ToList();
                }
                else
                {
                    filteredTokens = filter.Split('+').Intersect(tokens).ToList();
                }
            }

            // Replace with wildcard (maybe too limited)
            if (filter.IsNotNullOrWhiteSpace() && filter.EndsWith("+") && filteredTokens.Count != tokens.Count)
            {
                filteredTokens.Add("--");
            }

            if (skipEnglishOnly && filteredTokens.Count == 1 && filteredTokens.First() == "EN")
            {
                return string.Empty;
            }

            var response = string.Join("+", filteredTokens);

            if (quoted && response.IsNotNullOrWhiteSpace())
            {
                return $"[{response}]";
            }
            else
            {
                return response;
            }
        }

        private void UpdateMediaInfoIfNeeded(string pattern, MovieFile movieFile, Movie movie)
        {
            if (movie.Path.IsNullOrWhiteSpace())
            {
                return;
            }

            var schemaRevision = movieFile.MediaInfo != null ? movieFile.MediaInfo.SchemaRevision : 0;
            var matches = TitleRegex.Matches(pattern);

            var shouldUpdateMediaInfo = matches.Cast<Match>()
                .Select(m => MinimumMediaInfoSchemaRevisions.GetValueOrDefault(m.Value, -1))
                .Any(r => schemaRevision < r);

            if (shouldUpdateMediaInfo)
            {
                _mediaInfoUpdater.Update(movieFile, movie);
            }
        }

        private string ReplaceTokens(string pattern, Dictionary<string, Func<TokenMatch, string>> tokenHandlers, NamingConfig namingConfig)
        {
            return TitleRegex.Replace(pattern, match => ReplaceToken(match, tokenHandlers, namingConfig));
        }

        private string ReplaceToken(Match match, Dictionary<string, Func<TokenMatch, string>> tokenHandlers, NamingConfig namingConfig)
        {
            var tokenMatch = new TokenMatch
            {
                RegexMatch = match,
                Tag = match.Groups["tag"].Value,
                Prefix = match.Groups["prefix"].Value,
                Separator = match.Groups["separator"].Value,
                Suffix = match.Groups["suffix"].Value,
                Token = match.Groups["token"].Value,
                CustomFormat = match.Groups["customFormat"].Value
            };

            if (tokenMatch.CustomFormat.IsNullOrWhiteSpace())
            {
                tokenMatch.CustomFormat = null;
            }

            var tokenHandler = tokenHandlers.GetValueOrDefault(tokenMatch.Token, m => string.Empty);

            var replacementText = tokenHandler(tokenMatch).Trim();

            if (tokenMatch.Token.All(t => !char.IsLetter(t) || char.IsLower(t)))
            {
                replacementText = replacementText.ToLower();
            }
            else if (tokenMatch.Token.All(t => !char.IsLetter(t) || char.IsUpper(t)))
            {
                replacementText = replacementText.ToUpper();
            }

            if (!tokenMatch.Separator.IsNullOrWhiteSpace())
            {
                replacementText = replacementText.Replace(" ", tokenMatch.Separator);
            }

            replacementText = CleanFileName(replacementText, namingConfig);

            if (!replacementText.IsNullOrWhiteSpace())
            {
                replacementText = tokenMatch.Tag + tokenMatch.Prefix + replacementText + tokenMatch.Suffix;
            }

            return replacementText;
        }

        private string ReplaceNumberToken(string token, int value)
        {
            var split = token.Trim('{', '}').Split(':');
            if (split.Length == 1)
            {
                return value.ToString("0");
            }

            return value.ToString(split[1]);
        }

        private string GetQualityProper(Movie movie, QualityModel quality)
        {
            if (quality.Revision.Version > 1)
            {
                return "Proper";
            }

            return string.Empty;
        }

        private string GetQualityReal(Movie movie, QualityModel quality)
        {
            if (quality.Revision.Real > 0)
            {
                return "REAL";
            }

            return string.Empty;
        }

        private string GetOriginalTitle(MovieFile movieFile, bool multipleTokens)
        {
            if (movieFile.SceneName.IsNullOrWhiteSpace())
            {
                return CleanFileName(GetOriginalFileName(movieFile, multipleTokens));
            }

            return CleanFileName(movieFile.SceneName);
        }

        private string GetOriginalFileName(MovieFile movieFile, bool multipleTokens)
        {
            if (multipleTokens)
            {
                return string.Empty;
            }

            if (movieFile.RelativePath.IsNullOrWhiteSpace())
            {
                return Path.GetFileNameWithoutExtension(movieFile.Path);
            }

            return Path.GetFileNameWithoutExtension(movieFile.RelativePath);
        }

        private string ReplaceReservedDeviceNames(string input)
        {
            // Replace reserved windows device names with an alternative
            return ReservedDeviceNamesRegex.Replace(input, match => match.Value.Replace(".", "_"));
        }

        private static string CleanFileName(string name, NamingConfig namingConfig)
        {
            var result = name;

            if (namingConfig.ReplaceIllegalCharacters)
            {
                // Smart replaces a colon followed by a space with space dash space for a better appearance
                if (namingConfig.ColonReplacementFormat == ColonReplacementFormat.Smart)
                {
                    result = result.Replace(": ", " - ");
                    result = result.Replace(":", "-");
                }
                else
                {
                    var replacement = string.Empty;

                    switch (namingConfig.ColonReplacementFormat)
                    {
                        case ColonReplacementFormat.Dash:
                            replacement = "-";
                            break;
                        case ColonReplacementFormat.SpaceDash:
                            replacement = " -";
                            break;
                        case ColonReplacementFormat.SpaceDashSpace:
                            replacement = " - ";
                            break;
                    }

                    result = result.Replace(":", replacement);
                }
            }
            else
            {
                result = result.Replace(":", string.Empty);
            }

            for (var i = 0; i < BadCharacters.Length; i++)
            {
                result = result.Replace(BadCharacters[i], namingConfig.ReplaceIllegalCharacters ? GoodCharacters[i] : string.Empty);
            }

            return result.TrimStart(' ', '.').TrimEnd(' ');
        }

        private string Truncate(string input, string formatter)
        {
            if (input.IsNullOrWhiteSpace())
            {
                return string.Empty;
            }

            var maxLength = GetMaxLengthFromFormatter(formatter);

            if (maxLength == 0 || input.Length <= Math.Abs(maxLength))
            {
                return input;
            }

            if (maxLength < 0)
            {
                return $"{{ellipsis}}{input.Reverse().Truncate(Math.Abs(maxLength) - 3).TrimEnd(' ', '.').Reverse()}";
            }

            return $"{input.Truncate(maxLength - 3).TrimEnd(' ', '.')}{{ellipsis}}";
        }

        private int GetMaxLengthFromFormatter(string formatter)
        {
            int.TryParse(formatter, out var maxCustomLength);

            return maxCustomLength;
        }
    }

    internal sealed class TokenMatch
    {
        public Match RegexMatch { get; set; }
        public string Tag { get; set; }
        public string Prefix { get; set; }
        public string Separator { get; set; }
        public string Suffix { get; set; }
        public string Token { get; set; }
        public string CustomFormat { get; set; }

        public string DefaultValue(string defaultValue)
        {
            if (string.IsNullOrEmpty(Prefix) && string.IsNullOrEmpty(Suffix))
            {
                return defaultValue;
            }
            else
            {
                return string.Empty;
            }
        }
    }

    public enum ColonReplacementFormat
    {
        Delete = 0,
        Dash = 1,
        SpaceDash = 2,
        SpaceDashSpace = 3,
        Smart = 4
    }
}
