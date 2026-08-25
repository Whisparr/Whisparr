using System;
using System.Collections.Generic;
using Equ;
using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Indexers.Fanzub
{
    public class FanzubSettingsValidator : AbstractValidator<FanzubSettings>
    {
        public FanzubSettingsValidator()
        {
            RuleFor(c => c.BaseUrl).ValidRootUrl();
        }
    }

    public class FanzubSettings : PropertywiseEquatable<FanzubSettings>, IIndexerSettings
    {
        private static readonly FanzubSettingsValidator Validator = new ();

        public FanzubSettings()
        {
            BaseUrl = "http://fanzub.com/rss/";
            FailDownloads = Array.Empty<int>();
        }

        [FieldDefinition(0, Label = "Rss URL", HelpText = "Enter to URL to an Fanzub compatible RSS feed")]
        public string BaseUrl { get; set; }

        [FieldDefinition(1, Label = "Anime Standard Format Search", Type = FieldType.Checkbox, HelpText = "Also search for anime using the standard numbering")]
        public bool AnimeStandardFormatSearch { get; set; }

        [FieldDefinition(2, Type = FieldType.Select, SelectOptions = typeof(FailDownloads), Label = "Fail Downloads", HelpText = "Mark downloads containing these file types as failed", Advanced = true)]
        public IEnumerable<int> FailDownloads { get; set; }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
