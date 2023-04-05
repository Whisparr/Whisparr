using System.Collections.Generic;
using NzbDrone.Core.SeriesStats;

namespace Whisparr.Api.V3.Movies
{
    public class MovieStatisticsResource
    {
        public long SizeOnDisk { get; set; }
        public List<string> ReleaseGroups { get; set; }
    }

    public static class MovieStatisticsResourceMapper
    {
        public static MovieStatisticsResource ToResource(this MovieStatistics model)
        {
            if (model == null)
            {
                return null;
            }

            return new MovieStatisticsResource
            {
                SizeOnDisk = model.SizeOnDisk,
                ReleaseGroups = model.ReleaseGroups
            };
        }
    }
}
