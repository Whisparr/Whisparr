using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.ImportLists.ImportExclusions;

namespace Whisparr.Api.V3.ImportLists
{
    public class ImportListExclusionResource : ProviderResource<ImportListExclusionResource>
    {
        // public int Id { get; set; }
        public string ForeignId { get; set; }
        public string MovieTitle { get; set; }
        public ImportExclusionType Type { get; set; }
        public int? MovieYear { get; set; }
    }

    public static class ImportListExclusionResourceMapper
    {
        public static ImportListExclusionResource ToResource(this ImportListExclusion model)
        {
            if (model == null)
            {
                return null;
            }

            return new ImportListExclusionResource
            {
                Id = model.Id,
                ForeignId = model.ForeignId,
                MovieTitle = model.MovieTitle,
                Type = model.Type,
                MovieYear = model.MovieYear
            };
        }

        public static List<ImportListExclusionResource> ToResource(this IEnumerable<ImportListExclusion> exclusions)
        {
            return exclusions.Select(ToResource).ToList();
        }

        public static ImportListExclusion ToModel(this ImportListExclusionResource resource)
        {
            return new ImportListExclusion
            {
                Id = resource.Id,
                ForeignId = resource.ForeignId,
                MovieTitle = resource.MovieTitle,
                Type = resource.Type,
                MovieYear = resource.MovieYear ?? 0
            };
        }

        public static List<ImportListExclusion> ToModel(this IEnumerable<ImportListExclusionResource> resources)
        {
            return resources.Select(ToModel).ToList();
        }
    }
}
