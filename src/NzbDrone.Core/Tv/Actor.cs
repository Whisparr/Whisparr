using System.Collections.Generic;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Tv
{
    public class Actor : IEmbeddedDocument
    {
        public Actor()
        {
            Images = new List<MediaCover.MediaCover>();
        }

        public int TpdbId { get; set; }
        public string Name { get; set; }
        public string Character { get; set; }
        public Gender Gender { get; set; }
        public List<MediaCover.MediaCover> Images { get; set; }
    }

    public enum Gender
    {
        Female,
        Male,
        Other
    }
}
