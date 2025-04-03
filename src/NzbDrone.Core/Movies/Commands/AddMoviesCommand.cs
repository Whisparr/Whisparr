using System.Collections.Generic;
using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.Movies.Commands
{
    public class AddMoviesCommand : Command
    {
        public List<Movie> Movies { get; set; }

        public AddMoviesCommand(List<Movie> movies)
        {
            Movies = movies;
        }
    }
}
