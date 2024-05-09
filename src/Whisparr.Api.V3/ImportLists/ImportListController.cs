using FluentValidation;
using NzbDrone.Core.ImportLists;
using NzbDrone.Core.Validation;
using NzbDrone.Core.Validation.Paths;
using Whisparr.Http;

namespace Whisparr.Api.V3.ImportLists
{
    [V3ApiController]
    public class ImportListController : ProviderControllerBase<ImportListResource, ImportListBulkResource, IImportList, ImportListDefinition>
    {
        public static readonly ImportListBulkResourceMapper BulkResourceMapper = new ();
        public ImportListController(
            IImportListFactory importListFactory,
            RootFolderExistsValidator rootFolderExistsValidator,
            QualityProfileExistsValidator qualityProfileExistsValidator,
            ImportListResourceMapper resourceMapper)
            : base(importListFactory, "importlist", resourceMapper, BulkResourceMapper)
        {
            SharedValidator.RuleFor(c => c.RootFolderPath).Cascade(CascadeMode.Stop)
                .IsValidPath()
                .SetValidator(rootFolderExistsValidator);

            SharedValidator.RuleFor(c => c.QualityProfileId).Cascade(CascadeMode.Stop)
                .ValidId()
                .SetValidator(qualityProfileExistsValidator);
        }
    }
}
