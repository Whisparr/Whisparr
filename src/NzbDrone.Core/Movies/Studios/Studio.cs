using System;
using System.Collections.Generic;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Movies.Studios
{
    public class Studio : ModelBase
    {
        public Studio()
        {
            Images = new List<MediaCover.MediaCover>();
            Tags = new HashSet<int>();
        }

        public string ForeignId { get; set; }
        public string Title { get; set; }
        public string SortTitle { get; set; }
        public string CleanTitle { get; set; }
        public List<string> Aliases { get; set; }
        public string SearchTitle { get; set; }
        public string CleanSearchTitle { get; set; }
        public string Website { get; set; }
        public List<MediaCover.MediaCover> Images { get; set; }
        public string RootFolderPath { get; set; }
        public string Network { get; set; }
        public DateTime Added { get; set; }
        public bool Monitored { get; set; }
        public DateTime? AfterDate { get; set; }
        public int QualityProfileId { get; set; }
        public bool SearchOnAdd { get; set; }
        public DateTime? LastInfoSync { get; set; }
        public HashSet<int> Tags { get; set; }

        public void ApplyChanges(Studio otherStudio)
        {
            SearchTitle = otherStudio.SearchTitle;
            CleanSearchTitle = otherStudio.CleanSearchTitle;
            QualityProfileId = otherStudio.QualityProfileId;
            SearchOnAdd = otherStudio.SearchOnAdd;
            Monitored = otherStudio.Monitored;
            AfterDate = otherStudio.AfterDate;

            RootFolderPath = otherStudio.RootFolderPath;
            Tags = otherStudio.Tags;
        }
    }
}
