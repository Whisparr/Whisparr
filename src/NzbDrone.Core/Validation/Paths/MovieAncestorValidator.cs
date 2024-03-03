using System;
using System.Linq;
using FluentValidation.Validators;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Movies;

namespace NzbDrone.Core.Validation.Paths
{
    public class MovieAncestorValidator : PropertyValidator
    {
        private readonly IMovieService _movieService;

        public MovieAncestorValidator(IMovieService movieService)
        {
            _movieService = movieService;
        }

        protected override string GetDefaultMessageTemplate() => "Path '{path}' is an ancestor of an existing movie";

        protected override bool IsValid(PropertyValidatorContext context)
        {
            if (context.PropertyValue == null)
            {
                return true;
            }

            dynamic instance = context.ParentContext.InstanceToValidate;

            context.MessageFormatter.AppendArgument("path", context.PropertyValue.ToString());

            try
            {
                if (instance.ItemType == ItemType.Movie)
                {
                    return !_movieService.AllMoviePaths().Any(s => context.PropertyValue.ToString().IsParentPath(s.Value));
                }
                else
                {
                    return true;
                }
            }
            catch (Exception)
            {
                return true;
            }
        }
    }
}
