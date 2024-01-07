using System.Collections.Generic;
using NzbDrone.Core.Movies.Performers;

namespace NzbDrone.Core.Movies
{
    public class Credit
    {
        public string CreditForeignId { get; set; }
        public string Department { get; set; }
        public string Job { get; set; }
        public string Character { get; set; }
        public int Order { get; set; }
        public CreditType Type { get; set; }
        public CreditPerformer Performer { get; set; }
    }

    public class CreditPerformer
    {
        public CreditPerformer()
        {
            Images = new List<MediaCover.MediaCover>();
        }

        public string Name { get; set; }
        public string ForeignId { get; set; }
        public Gender Gender { get; set; }
        public List<MediaCover.MediaCover> Images { get; set; }
    }

    public enum CreditType
    {
        Cast,
        Crew
    }
}
