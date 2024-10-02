using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using FluentValidation;
using FluentValidation.Validators;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Core.Organizer
{
    public static class FileNameValidation
    {
        public static readonly Regex DeprecatedMovieFolderTokensRegex = new (@"(\{[- ._\[\(]?(?:Original[- ._](?:Title|Filename)|Release[- ._]Group|Edition[- ._]Tags|Quality[- ._](?:Full|Title|Proper|Real)|MediaInfo[- ._](?:Video|VideoCodec|VideoBitDepth|Audio|AudioCodec|AudioChannels|AudioLanguages|AudioLanguagesAll|SubtitleLanguages|SubtitleLanguagesAll|3D|Simple|Full|VideoDynamicRange|VideoDynamicRangeType))[- ._\]\)]?\})",
                                                                            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static IRuleBuilderOptions<T, string> ValidMovieFolderFormat<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            ruleBuilder.SetValidator(new NotEmptyValidator(null));
            ruleBuilder.SetValidator(new IllegalCharactersValidator());
            ruleBuilder.SetValidator(new IllegalMovieFolderTokensValidator());

            return ruleBuilder.SetValidator(new RegularExpressionValidator(FileNameBuilder.MovieTitleRegex)).WithMessage("Must contain movie title");
        }

        public static IRuleBuilderOptions<T, string> ValidMainMovieFolderFormat<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            ruleBuilder.SetValidator(new NotEmptyValidator(null));
            ruleBuilder.SetValidator(new IllegalCharactersValidator());

            return ruleBuilder.SetValidator(new RegularExpressionValidator(FileNameBuilder.MainFolderRegex)).WithMessage("Must start with a relative path inside root folder, ex. 'movies/'");
        }

        public static IRuleBuilderOptions<T, string> ValidMovieFormat<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            ruleBuilder.SetValidator(new NotEmptyValidator(null));
            ruleBuilder.SetValidator(new IllegalCharactersValidator());

            return ruleBuilder.SetValidator(new RegularExpressionValidator(FileNameBuilder.MovieTitleRegex)).WithMessage("Must contain movie title");
        }

        public static IRuleBuilderOptions<T, string> ValidSceneFolderFormat<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            ruleBuilder.SetValidator(new NotEmptyValidator(null));
            ruleBuilder.SetValidator(new IllegalCharactersValidator());
            ruleBuilder.SetValidator(new IllegalMovieFolderTokensValidator());

            return ruleBuilder.SetValidator(new RegularExpressionValidator(FileNameBuilder.SceneFolderRegex)).WithMessage("Must contain scene studio");
        }

        public static IRuleBuilderOptions<T, string> ValidMainSceneFolderFormat<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            ruleBuilder.SetValidator(new NotEmptyValidator(null));
            ruleBuilder.SetValidator(new IllegalCharactersValidator());

            return ruleBuilder.SetValidator(new RegularExpressionValidator(FileNameBuilder.MainFolderRegex)).WithMessage("Must start with a relative path inside root folder, ex. 'scenes/'");
        }

        public static IRuleBuilderOptions<T, string> ValidSceneImportFolderFormat<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            ruleBuilder.SetValidator(new NotEmptyValidator(null));
            ruleBuilder.SetValidator(new IllegalCharactersValidator());

            return ruleBuilder.SetValidator(new NotEmptyValidator(null)).WithMessage("Must not be empty");
        }

        public static IRuleBuilderOptions<T, string> ValidSceneFormat<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            ruleBuilder.SetValidator(new NotEmptyValidator(null));
            ruleBuilder.SetValidator(new IllegalCharactersValidator());

            return ruleBuilder.SetValidator(new RegularExpressionValidator(FileNameBuilder.SceneTitleRegex)).WithMessage("Must contain scene title");
        }
    }

    public class ValidMovieFolderFormatValidator : PropertyValidator
    {
        protected override string GetDefaultMessageTemplate() => "Must contain movie title";

        protected override bool IsValid(PropertyValidatorContext context)
        {
            if (context.PropertyValue is not string value)
            {
                return false;
            }

            return FileNameBuilder.MovieTitleRegex.IsMatch(value);
        }
    }

    public class IllegalMovieFolderTokensValidator : PropertyValidator
    {
        protected override string GetDefaultMessageTemplate() => "Must not contain deprecated tokens derived from file properties: {tokens}";

        protected override bool IsValid(PropertyValidatorContext context)
        {
            if (context.PropertyValue is not string value)
            {
                return false;
            }

            var match = FileNameValidation.DeprecatedMovieFolderTokensRegex.Matches(value);

            if (match.Any())
            {
                context.MessageFormatter.AppendArgument("tokens", string.Join(", ", match.Select(c => c.Value).ToArray()));

                return false;
            }

            return true;
        }
    }

    public class IllegalCharactersValidator : PropertyValidator
    {
        private static readonly char[] InvalidPathChars = Path.GetInvalidPathChars();

        protected override string GetDefaultMessageTemplate() => "Contains illegal characters: {InvalidCharacters}";

        protected override bool IsValid(PropertyValidatorContext context)
        {
            var value = context.PropertyValue as string;
            if (value.IsNullOrWhiteSpace())
            {
                return true;
            }

            var invalidCharacters = InvalidPathChars.Where(i => value!.IndexOf(i) >= 0).ToList();
            if (invalidCharacters.Any())
            {
                context.MessageFormatter.AppendArgument("InvalidCharacters", string.Join("", invalidCharacters));
                return false;
            }

            return true;
        }
    }
}
