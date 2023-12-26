using System;
using System.Collections.Generic;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Movies.Performers
{
    public class Performer : ModelBase
    {
        public Performer()
        {
            Images = new List<MediaCover.MediaCover>();
            Tags = new HashSet<int>();
        }

        public string ForeignId { get; set; }
        public string Name { get; set; }
        public string SortName { get; set; }
        public string CleanName { get; set; }
        public List<MediaCover.MediaCover> Images { get; set; }
        public Gender Gender { get; set; }
        public string RootFolderPath { get; set; }
        public DateTime Added { get; set; }
        public bool Monitored { get; set; }
        public int QualityProfileId { get; set; }
        public bool SearchOnAdd { get; set; }
        public DateTime? LastInfoSync { get; set; }
        public HashSet<int> Tags { get; set; }
    }

    public enum Gender
    {
        Male,
        Female,
        TransMale,
        TransFemale,
        Intersex,
        NonBinary
    }
}
