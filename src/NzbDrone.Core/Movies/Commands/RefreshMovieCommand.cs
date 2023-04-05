using System.Collections.Generic;
using System.Text.Json.Serialization;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Core.Movies.Commands
{
    public class RefreshMovieCommand : Command
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public int MovieId
        {
            get => 0;
            set
            {
                if (MovieIds.Empty())
                {
                    MovieIds.Add(value);
                }
            }
        }

        public List<int> MovieIds { get; set; }
        public bool IsNewMovie { get; set; }

        public RefreshMovieCommand()
        {
            MovieIds = new List<int>();
        }

        public RefreshMovieCommand(List<int> movieIds, bool isNewMovie = false)
        {
            MovieIds = movieIds;
            IsNewMovie = isNewMovie;
        }

        public override bool SendUpdatesToClient => true;

        public override bool UpdateScheduledTask => MovieIds.Empty();

        public override bool IsLongRunning => true;
    }
}
