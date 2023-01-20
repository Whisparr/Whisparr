using System.Collections.Generic;

namespace NzbDrone.Core.MetadataSource.SkyHook.Resource
{
    public class ShowResource
    {
        public ShowResource()
        {
            Genres = new List<string>();
            Images = new List<ImageResource>();
            Years = new List<int>();
            Episodes = new List<EpisodeResource>();
        }

        public int ForeignId { get; set; }
        public string Title { get; set; }
        public string Overview { get; set; }

        // public string Language { get; set; }
        public string Slug { get; set; }
        public string FirstAired { get; set; }

        public string Status { get; set; }
        public int? Runtime { get; set; }
        public TimeOfDayResource TimeOfDay { get; set; }

        public string Network { get; set; }
        public string ImdbId { get; set; }
        public string OriginalLanguage { get; set; }
        public List<string> Genres { get; set; }

        public string ContentRating { get; set; }

        public RatingResource Rating { get; set; }

        public List<ImageResource> Images { get; set; }
        public List<int> Years { get; set; }
        public List<EpisodeResource> Episodes { get; set; }
    }
}
