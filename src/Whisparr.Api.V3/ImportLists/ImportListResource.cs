using System;
using NzbDrone.Core.ImportLists;
using NzbDrone.Core.Tv;

namespace Whisparr.Api.V3.ImportLists
{
    public class ImportListResource : ProviderResource<ImportListResource>
    {
        public bool EnableAutomaticAdd { get; set; }
        public MonitorTypes ShouldMonitor { get; set; }
        public string RootFolderPath { get; set; }
        public int QualityProfileId { get; set; }
        public ImportListType ListType { get; set; }
        public int ListOrder { get; set; }
        public TimeSpan MinRefreshInterval { get; set; }
    }

    public class ImportListResourceMapper : ProviderResourceMapper<ImportListResource, ImportListDefinition>
    {
        public override ImportListResource ToResource(ImportListDefinition definition)
        {
            if (definition == null)
            {
                return null;
            }

            var resource = base.ToResource(definition);

            resource.EnableAutomaticAdd = definition.EnableAutomaticAdd;
            resource.ShouldMonitor = definition.ShouldMonitor;
            resource.RootFolderPath = definition.RootFolderPath;
            resource.QualityProfileId = definition.QualityProfileId;
            resource.ListType = definition.ListType;
            resource.ListOrder = (int)definition.ListType;
            resource.MinRefreshInterval = definition.MinRefreshInterval;

            return resource;
        }

        public override ImportListDefinition ToModel(ImportListResource resource, ImportListDefinition existingDefinition)
        {
            if (resource == null)
            {
                return null;
            }

            var definition = base.ToModel(resource, existingDefinition);

            definition.EnableAutomaticAdd = resource.EnableAutomaticAdd;
            definition.ShouldMonitor = resource.ShouldMonitor;
            definition.RootFolderPath = resource.RootFolderPath;
            definition.QualityProfileId = resource.QualityProfileId;
            definition.ListType = resource.ListType;
            definition.MinRefreshInterval = resource.MinRefreshInterval;

            return definition;
        }
    }
}
