using System;
using FluentValidation.Validators;
using NzbDrone.Core.Movies;

namespace NzbDrone.Core.Validation.Paths
{
    public class MovieExistsValidator : PropertyValidator
    {
        private readonly IMovieService _movieService;

        public MovieExistsValidator(IMovieService movieService)
        {
            _movieService = movieService;
        }

        protected override string GetDefaultMessageTemplate() => "This movie has already been added";

        protected override bool IsValid(PropertyValidatorContext context)
        {
            if (context.PropertyValue == null)
            {
                return true;
            }

            var tmdbId = Convert.ToInt32(context.PropertyValue.ToString());

            return !_movieService.GetAllMovies().Exists(s => s.TmdbId == tmdbId);
        }
    }
}
