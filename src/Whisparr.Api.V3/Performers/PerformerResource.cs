using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.Movies.Performers;
using Whisparr.Http.REST;

namespace Whisparr.Api.V3.Performers
{
    public class PerformerResource : RestResource
    {
        public string Name { get; set; }
        public Gender Gender { get; set; }
        public string ForeignId { get; set; }
        public List<MediaCover> Images { get; set; }
        public bool Monitored { get; set; }
        public string RootFolderPath { get; set; }
        public int QualityProfileId { get; set; }
        public bool SearchOnAdd { get; set; }
        public HashSet<int> Tags { get; set; }
    }

    public static class PerformerResourceMapper
    {
        public static PerformerResource ToResource(this Performer model)
        {
            if (model == null)
            {
                return null;
            }

            return new PerformerResource
            {
                Id = model.Id,
                ForeignId = model.ForeignId,
                Gender = model.Gender,
                Name = model.Name,
                Monitored = model.Monitored,
                Images = model.Images,
                QualityProfileId = model.QualityProfileId,
                RootFolderPath = model.RootFolderPath,
                SearchOnAdd = model.SearchOnAdd,
                Tags = model.Tags
            };
        }

        public static List<PerformerResource> ToResource(this IEnumerable<Performer> collections)
        {
            return collections.Select(ToResource).ToList();
        }

        public static Performer ToModel(this PerformerResource resource)
        {
            if (resource == null)
            {
                return null;
            }

            return new Performer
            {
                Id = resource.Id,
                ForeignId = resource.ForeignId,
                Name = resource.Name,
                Monitored = resource.Monitored,
                QualityProfileId = resource.QualityProfileId,
                RootFolderPath = resource.RootFolderPath,
                SearchOnAdd = resource.SearchOnAdd,
                Tags = resource.Tags
            };
        }
    }
}
