using FluentValidation;
using NzbDrone.Core.ImportLists;
using NzbDrone.Core.Validation;
using NzbDrone.Core.Validation.Paths;
using NzbDrone.SignalR;
using Whisparr.Http;

namespace Whisparr.Api.V3.ImportLists
{
    [V3ApiController]
    public class ImportListController : ProviderControllerBase<ImportListResource, ImportListBulkResource, IImportList, ImportListDefinition>
    {
        public static readonly ImportListResourceMapper ResourceMapper = new ();
        public static readonly ImportListBulkResourceMapper BulkResourceMapper = new ();

        public ImportListController(IBroadcastSignalRMessage signalRBroadcaster,
            IImportListFactory importListFactory,
            RootFolderExistsValidator rootFolderExistsValidator,
            ProfileExistsValidator profileExistsValidator)
            : base(signalRBroadcaster, importListFactory, "importlist", ResourceMapper, BulkResourceMapper)
        {
            SharedValidator.RuleFor(c => c.RootFolderPath).Cascade(CascadeMode.Stop)
                .IsValidPath()
                .SetValidator(rootFolderExistsValidator);

            SharedValidator.RuleFor(c => c.QualityProfileId).SetValidator(profileExistsValidator);
        }
    }
}
