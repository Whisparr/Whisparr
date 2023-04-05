using FluentValidation;
using FluentValidation.Results;
using NzbDrone.Core.Validation.Paths;

namespace NzbDrone.Core.Movies
{
    public interface IAddMovieValidator
    {
        ValidationResult Validate(Movie instance);
    }

    public class AddMovieValidator : AbstractValidator<Movie>, IAddMovieValidator
    {
        // TODO Rework these for Movies
        public AddMovieValidator(RootFolderValidator rootFolderValidator,
                                  MediaPathValidator mediaPathValidator,
                                  MediaAncestorValidator mediaAncestorValidator)
        {
            RuleFor(c => c.Path).Cascade(CascadeMode.Stop)
                .IsValidPath()
                                .SetValidator(rootFolderValidator)
                                .SetValidator(mediaPathValidator)
                                .SetValidator(mediaAncestorValidator);
        }
    }
}
