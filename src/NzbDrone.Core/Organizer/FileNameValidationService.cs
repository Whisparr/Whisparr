using System.Collections.Generic;
using System.Linq;
using FluentValidation.Results;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Organizer
{
    public interface IFilenameValidationService
    {
        ValidationFailure ValidateStandardFilename(SampleResult sampleResult);
    }

    public class FileNameValidationService : IFilenameValidationService
    {
        private const string ERROR_MESSAGE = "Produces invalid file names";

        public ValidationFailure ValidateStandardFilename(SampleResult sampleResult)
        {
            var validationFailure = new ValidationFailure("StandardEpisodeFormat", ERROR_MESSAGE);
            var parsedEpisodeInfo = Parser.Parser.ParseTitle(sampleResult.FileName);

            if (parsedEpisodeInfo == null)
            {
                return validationFailure;
            }

            if (!parsedEpisodeInfo.AirDate.Equals(sampleResult.Episodes.Single().AirDate))
            {
                return validationFailure;
            }

            return null;
        }
    }
}
