using NzbDrone.Core.Configuration;
using Whisparr.Http;
using Whisparr.Http.Validation;

namespace Whisparr.Api.V3.Config
{
    [V3ApiController("config/importlist")]

    public class ImportListConfigController : ConfigController<ImportListConfigResource>
    {
        public ImportListConfigController(IConfigService configService)
            : base(configService)
        {
            SharedValidator.RuleFor(c => c.ImportListSyncInterval)
               .IsValidImportListSyncInterval();
        }

        protected override ImportListConfigResource ToResource(IConfigService model)
        {
            return ImportListConfigResourceMapper.ToResource(model);
        }
    }
}
