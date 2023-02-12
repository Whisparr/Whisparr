using Microsoft.AspNetCore.Mvc;
using NzbDrone.Core.Localization;
using Whisparr.Http;
using Whisparr.Http.REST;

namespace Whisparr.Api.V3.Localization
{
    [V3ApiController]
    public class LocalizationController : RestController<LocalizationResource>
    {
        private readonly ILocalizationService _localizationService;

        public LocalizationController(ILocalizationService localizationService)
        {
            _localizationService = localizationService;
        }

        protected override LocalizationResource GetResourceById(int id)
        {
            return GetLocalization();
        }

        [HttpGet]
        [Produces("application/json")]
        public LocalizationResource GetLocalization()
        {
            return _localizationService.GetLocalizationDictionary().ToResource();
        }

        [HttpGet("language")]
        [Produces("application/json")]
        public LocalizationLanguageResource GetLanguage()
        {
            var identifier = _localizationService.GetLanguageIdentifier();

            return new LocalizationLanguageResource
            {
                Identifier = identifier
            };
        }
    }
}
