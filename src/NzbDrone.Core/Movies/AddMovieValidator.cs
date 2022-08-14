using FluentValidation;
using FluentValidation.Results;
using NzbDrone.Core.Validation.Paths;

namespace NzbDrone.Core.Movies
{
    public interface IAddMovieValidator
    {
        ValidationResult Validate(Media instance);
    }

    public class AddMovieValidator : AbstractValidator<Media>, IAddMovieValidator
    {
        public AddMovieValidator(RootFolderValidator rootFolderValidator,
                                 RecycleBinValidator recycleBinValidator,
                                 MoviePathValidator moviePathValidator,
                                 MovieAncestorValidator movieAncestorValidator)
        {
            RuleFor(c => c.Path).Cascade(CascadeMode.StopOnFirstFailure)
                                .IsValidPath()
                                .SetValidator(rootFolderValidator)
                                .SetValidator(recycleBinValidator)
                                .SetValidator(moviePathValidator)
                                .SetValidator(movieAncestorValidator);
        }
    }
}
