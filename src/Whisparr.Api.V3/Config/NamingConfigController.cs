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
    [V3ApiController("config/naming")]
    public class NamingConfigController : RestController<NamingConfigResource>
    {
        private readonly INamingConfigService _namingConfigService;
        private readonly IFilenameSampleService _filenameSampleService;
        private readonly IFilenameValidationService _filenameValidationService;

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

        [HttpGet]
        public NamingConfigResource GetNamingConfig()
        {
            var nameSpec = _namingConfigService.GetConfig();
            var resource = nameSpec.ToResource();

            return resource;
        }

        [RestPutById]
        public ActionResult<NamingConfigResource> UpdateNamingConfig([FromBody] NamingConfigResource resource)
        {
            var nameSpec = resource.ToModel();
            ValidateFormatResult(nameSpec);

            _namingConfigService.Save(nameSpec);

            return Accepted(resource.Id);
        }

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
                : _filenameSampleService.GetSceneImportFolderSample(nameSpec.SceneImportFolderFormat);

            sampleResource.MaxFilePathLengthExample = NamingConfig.Default.MaxFilePathLength;
            sampleResource.MaxFolderPathLengthExample = NamingConfig.Default.MaxFolderPathLength;

            return sampleResource;
        }

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
