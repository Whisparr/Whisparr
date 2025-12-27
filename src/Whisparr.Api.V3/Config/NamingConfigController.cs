using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Organizer;
using Whisparr.Http;
using Whisparr.Http.REST;
using Whisparr.Http.REST.Attributes;

namespace Whisparr.Api.V3.Config
{
    /// <summary>
    /// Controller for managing naming configuration and generating naming examples.
    /// Exposes endpoints to retrieve and update the naming configuration and to
    /// produce filename/folder examples based on current settings.
    /// </summary>
    [V3ApiController("config/naming")]
    public class NamingConfigController : RestController<NamingConfigResource>
    {
        private readonly INamingConfigService _namingConfigService;
        private readonly IFilenameSampleService _filenameSampleService;
        private readonly IFilenameValidationService _filenameValidationService;

        /// <summary>
        /// Creates a new <see cref="NamingConfigController"/>.
        /// </summary>
        /// <param name="namingConfigService">Service to get and save naming configuration.</param>
        /// <param name="filenameSampleService">Service to generate filename and folder samples from a naming config.</param>
        /// <param name="filenameValidationService">Service to validate generated filenames against constraints.</param>
        public NamingConfigController(INamingConfigService namingConfigService,
                                      IFilenameSampleService filenameSampleService,
                                      IFilenameValidationService filenameValidationService)
        {
            _namingConfigService = namingConfigService;
            _filenameSampleService = filenameSampleService;
            _filenameValidationService = filenameValidationService;

            SharedValidator.RuleFor(c => c.StandardMovieFormat).ValidMovieFormat();
            SharedValidator.RuleFor(c => c.MovieFolderFormat).ValidMovieFolderFormat();
            SharedValidator.RuleFor(c => c.MovieFolderFormat).ValidMainMovieFolderFormat();
            SharedValidator.RuleFor(c => c.StandardSceneFormat).ValidSceneFormat();
            SharedValidator.RuleFor(c => c.SceneFolderFormat).ValidSceneFolderFormat();
            SharedValidator.RuleFor(c => c.SceneFolderFormat).ValidMainSceneFolderFormat();
            SharedValidator.RuleFor(c => c.SceneImportFolderFormat).ValidSceneImportFolderFormat();
        }

        protected override NamingConfigResource GetResourceById(int id)
        {
            return GetNamingConfig();
        }

        /// <summary>
        /// Returns the current naming configuration.
        /// </summary>
        /// <returns>The current <see cref="NamingConfigResource"/>.</returns>
        [HttpGet]
        public NamingConfigResource GetNamingConfig()
        {
            var nameSpec = _namingConfigService.GetConfig();
            var resource = nameSpec.ToResource();

            return resource;
        }

        /// <summary>
        /// Updates the naming configuration.
        /// Validates generated filename samples and saves the configuration if valid.
        /// </summary>
        /// <param name="resource">The naming configuration resource to save.</param>
        /// <returns>Accepted result containing the resource id on success.</returns>
        /// <exception cref="FluentValidation.ValidationException">Thrown when generated filename validation fails.</exception>
        [RestPutById]
        public ActionResult<NamingConfigResource> UpdateNamingConfig([FromBody] NamingConfigResource resource)
        {
            var nameSpec = resource.ToModel();
            ValidateFormatResult(nameSpec);

            _namingConfigService.Save(nameSpec);

            return Accepted(resource.Id);
        }

        /// <summary>
        /// Returns filename and folder examples generated from the provided or current naming configuration.
        /// </summary>
        /// <param name="config">Optional naming configuration; when omitted the current config is used.</param>
        /// <returns>An object containing example filenames and folder names and path length examples.</returns>
        [HttpGet("examples")]
        public object GetExamples([FromQuery]NamingConfigResource config)
        {
            if (config.Id == 0)
            {
                config = GetNamingConfig();
            }

            var nameSpec = config.ToModel();
            var sampleResource = new NamingExampleResource();

            var movieSampleResult = _filenameSampleService.GetMovieSample(nameSpec);
            var sceneSampleResult = _filenameSampleService.GetSceneSample(nameSpec);

            sampleResource.MovieExample = nameSpec.StandardMovieFormat.IsNullOrWhiteSpace()
                ? null
                : movieSampleResult.FileName;

            sampleResource.MovieFolderExample = nameSpec.MovieFolderFormat.IsNullOrWhiteSpace()
                ? null
                : _filenameSampleService.GetMovieFolderSample(nameSpec);

            sampleResource.SceneExample = nameSpec.StandardSceneFormat.IsNullOrWhiteSpace()
                ? "Invalid Format"
                : sceneSampleResult.FileName;

            sampleResource.SceneFolderExample = nameSpec.SceneFolderFormat.IsNullOrWhiteSpace()
                ? "Invalid format"
                : _filenameSampleService.GetSceneFolderSample(nameSpec);

            sampleResource.MainSceneFolderExample = nameSpec.SceneFolderFormat.IsNullOrWhiteSpace()
                ? "Invalid format"
                : _filenameSampleService.GetMovieFolderSample(nameSpec);

            sampleResource.SceneImportFolderExample = nameSpec.SceneImportFolderFormat.IsNullOrWhiteSpace()
                ? "Invalid format"
                : _filenameSampleService.GetMovieFolderSample(nameSpec);

            sampleResource.MaxFilePathLengthExample = NamingConfig.Default.MaxFilePathLength;
            sampleResource.MaxFolderPathLengthExample = NamingConfig.Default.MaxFolderPathLength;

            return sampleResource;
        }

        /// <summary>
        /// Validates a generated filename sample from the given naming configuration.
        /// If validation failures are present a <see cref="FluentValidation.ValidationException"/> is thrown.
        /// </summary>
        /// <param name="nameSpec">The naming configuration to validate.</param>
        private void ValidateFormatResult(NamingConfig nameSpec)
        {
            var movieSampleResult = _filenameSampleService.GetMovieSample(nameSpec);

            var standardMovieValidationResult = _filenameValidationService.ValidateMovieFilename(movieSampleResult);

            var validationFailures = new List<ValidationFailure>();

            validationFailures.AddIfNotNull(standardMovieValidationResult);

            if (validationFailures.Any())
            {
                throw new ValidationException(validationFailures.DistinctBy(v => v.PropertyName).ToArray());
            }
        }
    }
}
