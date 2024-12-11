using NzbDrone.Core.Configuration;
using Whisparr.Http.REST;

namespace Whisparr.Api.V3.Config
{
    public class IndexerConfigResource : RestResource
    {
        public int MinimumAge { get; set; }
        public int MaximumSize { get; set; }
        public int Retention { get; set; }
        public int RssSyncInterval { get; set; }
        public bool PreferIndexerFlags { get; set; }
        public int AvailabilityDelay { get; set; }
        public bool AllowHardcodedSubs { get; set; }
        public string WhitelistedHardcodedSubs { get; set; }
        public bool SearchTitleOnly { get; set; }
        public bool SearchTitleDate { get; set; }
        public bool SearchStudioDate { get; set; }
        public bool SearchStudioTitle { get; set; }
        public SearchDateFormatType SearchDateFormat { get; set; }
        public SearchStudioFormatType SearchStudioFormat { get; set; }
    }

    public static class IndexerConfigResourceMapper
    {
        public static IndexerConfigResource ToResource(IConfigService model)
        {
            return new IndexerConfigResource
            {
                MinimumAge = model.MinimumAge,
                MaximumSize = model.MaximumSize,
                Retention = model.Retention,
                RssSyncInterval = model.RssSyncInterval,
                PreferIndexerFlags = model.PreferIndexerFlags,
                AvailabilityDelay = model.AvailabilityDelay,
                AllowHardcodedSubs = model.AllowHardcodedSubs,
                WhitelistedHardcodedSubs = model.WhitelistedHardcodedSubs,
                SearchTitleOnly = model.SearchTitleOnly,
                SearchTitleDate = model.SearchTitleDate,
                SearchStudioDate = model.SearchStudioDate,
                SearchStudioTitle = model.SearchStudioTitle,
                SearchDateFormat = model.SearchDateFormat,
                SearchStudioFormat = model.SearchStudioFormat,
            };
        }
    }
}
