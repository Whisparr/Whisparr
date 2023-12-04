using NzbDrone.Core.ImportLists;
using NzbDrone.Core.Validation;
using NzbDrone.Core.Validation.Paths;
using Whisparr.Http;

namespace Whisparr.Api.V3.ImportLists
{
    [V3ApiController]
    public class ImportListController : ProviderControllerBase<ImportListResource, ImportListBulkResource, IImportList, ImportListDefinition>
    {
        public static readonly ImportListResourceMapper ResourceMapper = new ();
        public static readonly ImportListBulkResourceMapper BulkResourceMapper = new ();

        public ImportListController(IImportListFactory importListFactory, QualityProfileExistsValidator qualityProfileExistsValidator)
            : base(importListFactory, "importlist", ResourceMapper, BulkResourceMapper)
        {
            SharedValidator.RuleFor(c => c.RootFolderPath).IsValidPath();
            SharedValidator.RuleFor(c => c.QualityProfileId).ValidId();
            SharedValidator.RuleFor(c => c.QualityProfileId).SetValidator(qualityProfileExistsValidator);
        }
    }
}
