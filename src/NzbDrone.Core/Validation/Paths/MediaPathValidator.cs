using System.Linq;
using FluentValidation.Validators;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Validation.Paths
{
    public class MediaPathValidator : PropertyValidator
    {
        private readonly ISeriesService _seriesService;
        private readonly IMovieService _movieService;

        public MediaPathValidator(ISeriesService seriesService, IMovieService movieService)
        {
            _seriesService = seriesService;
            _movieService = movieService;
        }

        protected override string GetDefaultMessageTemplate() => "Path '{path}' is already configured for another series or movie";

        protected override bool IsValid(PropertyValidatorContext context)
        {
            if (context.PropertyValue == null)
            {
                return true;
            }

            context.MessageFormatter.AppendArgument("path", context.PropertyValue.ToString());

            dynamic instance = context.ParentContext.InstanceToValidate;
            var instanceId = (int)instance.Id;

            var configuredSeries = _seriesService.GetAllSeriesPaths().Any(s => s.Value.PathEquals(context.PropertyValue.ToString()) && s.Key != instanceId);
            var configuredMovie = _movieService.GetAllMoviePaths().Any(s => s.Value.PathEquals(context.PropertyValue.ToString()) && s.Key != instanceId);

            return !(configuredMovie || configuredSeries);
        }
    }
}
