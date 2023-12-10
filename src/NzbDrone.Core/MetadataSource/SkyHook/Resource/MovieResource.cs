using System;
using System.Collections.Generic;
using NzbDrone.Core.Movies;

namespace NzbDrone.Core.MetadataSource.SkyHook.Resource
{
    public class MovieResource
    {
        public ExternalIdResource ForeignIds { get; set; }
        public string Overview { get; set; }
        public string Title { get; set; }
        public string Slug { get; set; }

        // Depricated but left in place until cache fills new object (MovieRatings)
        public RatingResource Ratings { get; set; }
        public int? Duration { get; set; }
        public List<ImageResource> Images { get; set; }
        public List<string> Genres { get; set; }

        public int Year { get; set; }
        public DateTime? ReleaseDateUtc { get; set; }

        public List<CastResource> Credits { get; set; }
        public StudioResource Studio { get; set; }

        public string Homepage { get; set; }
        public ItemType ItemType { get; set; }
    }
}
