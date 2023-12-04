using System;
using System.Collections.Generic;

namespace NzbDrone.Core.ImportLists.Whisparr
{
    public class WhisparrMovie
    {
        public string Title { get; set; }
        public string SortTitle { get; set; }
        public int TmdbId { get; set; }
        public string Overview { get; set; }
        public List<MediaCover.MediaCover> Images { get; set; }
        public bool Monitored { get; set; }
        public DateTime InCinemas { get; set; }
        public DateTime PhysicalRelease { get; set; }
        public int Year { get; set; }
        public string TitleSlug { get; set; }
        public int QualityProfileId { get; set; }
        public string Path { get; set; }
        public HashSet<int> Tags { get; set; }
    }

    public class WhisparrProfile
    {
        public string Name { get; set; }
        public int Id { get; set; }
    }

    public class WhisparrTag
    {
        public string Label { get; set; }
        public int Id { get; set; }
    }

    public class WhisparrRootFolder
    {
        public string Path { get; set; }
        public int Id { get; set; }
    }
}
