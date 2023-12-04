using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.Movies.Studios;
using Whisparr.Http.REST;

namespace Whisparr.Api.V3.Studios
{
    public class StudioResource : RestResource
    {
        public string Title { get; set; }
        public string SortTitle { get; set; }
        public string ForeignId { get; set; }
        public string Website { get; set; }
        public List<MediaCover> Images { get; set; }
        public bool Monitored { get; set; }
        public string RootFolderPath { get; set; }
        public int QualityProfileId { get; set; }
        public bool SearchOnAdd { get; set; }
        public HashSet<int> Tags { get; set; }
    }

    public static class StudioResourceMapper
    {
        public static StudioResource ToResource(this Studio model)
        {
            if (model == null)
            {
                return null;
            }

            return new StudioResource
            {
                Id = model.Id,
                ForeignId = model.ForeignId,
                Title = model.Title,
                SortTitle = model.SortTitle,
                Website = model.Website,
                Monitored = model.Monitored,
                Images = model.Images,
                QualityProfileId = model.QualityProfileId,
                RootFolderPath = model.RootFolderPath,
                SearchOnAdd = model.SearchOnAdd,
                Tags = model.Tags
            };
        }

        public static List<StudioResource> ToResource(this IEnumerable<Studio> collections)
        {
            return collections.Select(ToResource).ToList();
        }

        public static Studio ToModel(this StudioResource resource)
        {
            if (resource == null)
            {
                return null;
            }

            return new Studio
            {
                Id = resource.Id,
                ForeignId = resource.ForeignId,
                Title = resource.Title,
                SortTitle = resource.SortTitle,
                Website = resource.Website,
                Monitored = resource.Monitored,
                QualityProfileId = resource.QualityProfileId,
                RootFolderPath = resource.RootFolderPath,
                SearchOnAdd = resource.SearchOnAdd,
                Tags = resource.Tags
            };
        }

        public static Studio ToModel(this StudioResource resource, Studio studio)
        {
            var updatedStudio = resource.ToModel();

            studio.ApplyChanges(updatedStudio);

            return studio;
        }
    }
}
