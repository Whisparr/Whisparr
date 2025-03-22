using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.Movies.Credits;
using Whisparr.Http.REST;

namespace NzbDrone.Api.V3.Credits
{
    public class CreditResource : RestResource
    {
        public CreditResource()
        {
            Images = new List<MediaCover>();
        }

        public string PersonName { get; set; }
        public string PerformerForeignId { get; set; }
        public int MovieMetadataId { get; set; }
        public List<MediaCover> Images { get; set; }
        public string Job { get; set; }
        public string Character { get; set; }
        public int Order { get; set; }
        public CreditType Type { get; set; }
    }

    public static class CreditResourceMapper
    {
        public static CreditResource ToResource(this Credit model)
        {
            if (model == null)
            {
                return null;
            }

            return new CreditResource
            {
                Id = model.Id,
                MovieMetadataId = model.MovieMetadataId,
                PerformerForeignId = model.PerformerForeignId,
                PersonName = model.Performer?.Name,
                Character = model.Character,
                Order = model.Order,
                Type = model.Type
            };
        }

        public static List<CreditResource> ToResource(this IEnumerable<Credit> credits)
        {
            return credits.Select(ToResource).ToList();
        }

        public static Credit ToModel(this CreditResource resource)
        {
            if (resource == null)
            {
                return null;
            }

            return new Credit
            {
                Id = resource.Id,
                MovieMetadataId = resource.MovieMetadataId,
                Character = resource.Character,
                PerformerForeignId = resource.PerformerForeignId,
                Order = resource.Order,
                Type = resource.Type
            };
        }
    }
}
