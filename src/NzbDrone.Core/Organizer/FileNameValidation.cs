using System;
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
        private static readonly Regex SeasonFolderRegex = new Regex(@"(\{season(\:\d+)?\})",
                                                                            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        internal static readonly Regex OriginalTokenRegex = new Regex(@"(\{original[- ._](?:title|filename)\})",
                                                                            RegexOptions.Compiled | RegexOptions.IgnoreCase);
        internal static readonly Regex ShortDateTokenRegex = new Regex(@"\{release[- _\.]?shortdate\}",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);
        internal static readonly Regex ExternalIdTokenRegex = new Regex(@"\{external[- _\.]?id\}",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static IRuleBuilderOptions<T, string> ValidEpisodeFormat<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            ruleBuilder.SetValidator(new NotEmptyValidator(null));
            ruleBuilder.SetValidator(new IllegalCharactersValidator());

            return ruleBuilder.SetValidator(new ValidStandardEpisodeFormatValidator());
        }

        public static IRuleBuilderOptions<T, string> ValidJavEpisodeFormat<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            ruleBuilder.SetValidator(new IllegalCharactersValidator());

            return ruleBuilder.SetValidator(new ValidJavEpisodeFormatValidator());
        }

        public static IRuleBuilderOptions<T, string> ValidSeriesFolderFormat<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            ruleBuilder.SetValidator(new NotEmptyValidator(null));
            ruleBuilder.SetValidator(new IllegalCharactersValidator());

            return ruleBuilder.SetValidator(new RegularExpressionValidator(FileNameBuilder.SeriesTitleRegex)).WithMessage("Must contain site title");
        }

        public static IRuleBuilderOptions<T, string> ValidSeasonFolderFormat<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            ruleBuilder.SetValidator(new NotEmptyValidator(null));
            ruleBuilder.SetValidator(new IllegalCharactersValidator());

            return ruleBuilder.SetValidator(new RegularExpressionValidator(SeasonFolderRegex)).WithMessage("Must contain season number");
        }

        public static IRuleBuilderOptions<T, string> ValidCustomColonReplacement<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            ruleBuilder.SetValidator(new IllegalColonCharactersValidator());

            return ruleBuilder.SetValidator(new IllegalCharactersValidator());
        }
    }

    public class ValidStandardEpisodeFormatValidator : PropertyValidator
    {
        protected override string GetDefaultMessageTemplate() => "Must contain Release Date, Release ShortDate OR Original Title";

        protected override bool IsValid(PropertyValidatorContext context)
        {
            if (context.PropertyValue is not string value)
            {
                return false;
            }

            return FileNameBuilder.AirDateRegex.IsMatch(value) ||
                   FileNameValidation.OriginalTokenRegex.IsMatch(value) ||
                   FileNameValidation.ShortDateTokenRegex.IsMatch(value);
        }
    }

    public class ValidJavEpisodeFormatValidator : PropertyValidator
    {
        protected override string GetDefaultMessageTemplate() => "Must contain External Id OR Original Title";

        protected override bool IsValid(PropertyValidatorContext context)
        {
            if (context.PropertyValue is not string value)
            {
                return true;
            }

            if (value.IsNullOrWhiteSpace())
            {
                return true;
            }

            return FileNameValidation.ExternalIdTokenRegex.IsMatch(value) ||
                   FileNameValidation.OriginalTokenRegex.IsMatch(value);
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

    public class IllegalColonCharactersValidator : PropertyValidator
    {
        private static readonly string[] InvalidPathChars = FileNameBuilder.BadCharacters.Concat(new[] { ":" }).ToArray();

        protected override string GetDefaultMessageTemplate() => "Contains illegal characters: {InvalidCharacters}";

        protected override bool IsValid(PropertyValidatorContext context)
        {
            var value = context.PropertyValue as string;

            if (value.IsNullOrWhiteSpace())
            {
                return true;
            }

            var invalidCharacters = InvalidPathChars.Where(i => value!.IndexOf(i, StringComparison.Ordinal) >= 0).ToList();

            if (invalidCharacters.Any())
            {
                context.MessageFormatter.AppendArgument("InvalidCharacters", string.Join("", invalidCharacters));
                return false;
            }

            return true;
        }
    }
}
