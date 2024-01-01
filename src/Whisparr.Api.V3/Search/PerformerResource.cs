using System.Collections.Generic;
using System.Linq;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.Movies.Performers;

namespace Whisparr.Api.V3.Search
{
    public class PerformerResource
    {
        public string Name { get; set; }
        public Gender Gender { get; set; }
        public List<MediaCover> Images { get; set; }
        public PerformerStatus Status { get; set; }
        public string RemotePoster { get; set; }
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
                Name = model.Name,
                Gender = model.Gender,
                Status = model.Status,
                Images = model.Images.JsonClone(),
            };
        }

        public static Performer ToModel(this PerformerResource resource)
        {
            if (resource == null)
            {
                return null;
            }

            return new Performer
            {
                Name = resource.Name,
                Images = resource.Images,
            };
        }

        public static List<PerformerResource> ToResource(this IEnumerable<Performer> movies)
        {
            return movies.Select(ToResource).ToList();
        }
    }
}
