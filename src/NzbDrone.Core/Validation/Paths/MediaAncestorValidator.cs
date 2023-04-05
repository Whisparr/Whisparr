using System.Linq;
using FluentValidation.Validators;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Validation.Paths
{
    public class MediaAncestorValidator : PropertyValidator
    {
        private readonly ISeriesService _seriesService;
        private readonly IMovieService _movieService;

        public MediaAncestorValidator(ISeriesService seriesService, IMovieService movieService)
        {
            _seriesService = seriesService;
            _movieService = movieService;
        }

        protected override string GetDefaultMessageTemplate() => "Path '{path}' is an ancestor of an existing series";

        protected override bool IsValid(PropertyValidatorContext context)
        {
            if (context.PropertyValue == null)
            {
                return true;
            }

            context.MessageFormatter.AppendArgument("path", context.PropertyValue.ToString());

            var seriesAncestor = _seriesService.GetAllSeriesPaths().Any(s => context.PropertyValue.ToString().IsParentPath(s.Value));
            var movieAncestor = _movieService.GetAllMoviePaths().Any(s => context.PropertyValue.ToString().IsParentPath(s.Value));

            return !(seriesAncestor || movieAncestor);
        }
    }
}
