using NzbDrone.Core.Configuration;
using Whisparr.Http.REST;

namespace Whisparr.Api.V3.Config
{
    public class ImportListConfigResource : RestResource
    {
        public string ListSyncLevel { get; set; }
        public string ImportExclusions { get; set; }
    }

    public static class ImportListConfigResourceMapper
    {
        public static ImportListConfigResource ToResource(IConfigService model)
        {
            return new ImportListConfigResource
            {
                ListSyncLevel = model.ListSyncLevel,
                ImportExclusions = model.ImportExclusions
            };
        }
    }
}
