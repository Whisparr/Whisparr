using System.Collections.Generic;
using System.Linq;
using Whisparr.Http.REST;

namespace Whisparr.Api.V3.Episodes
{
    public class RenameEpisodeResource : RestResource
    {
        public int SeriesId { get; set; }
        public int SeasonNumber { get; set; }
        public List<string> ReleaseDates { get; set; }
        public int EpisodeFileId { get; set; }
        public string ExistingPath { get; set; }
        public string NewPath { get; set; }
    }

    public static class RenameEpisodeResourceMapper
    {
        public static RenameEpisodeResource ToResource(this NzbDrone.Core.MediaFiles.RenameEpisodeFilePreview model)
        {
            if (model == null)
            {
                return null;
            }

            return new RenameEpisodeResource
            {
                SeriesId = model.SeriesId,
                SeasonNumber = model.SeasonNumber,
                ReleaseDates = model.ReleaseDates.ToList(),
                EpisodeFileId = model.EpisodeFileId,
                ExistingPath = model.ExistingPath,
                NewPath = model.NewPath
            };
        }

        public static List<RenameEpisodeResource> ToResource(this IEnumerable<NzbDrone.Core.MediaFiles.RenameEpisodeFilePreview> models)
        {
            return models.Select(ToResource).ToList();
        }
    }
}
