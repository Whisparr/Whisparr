using NzbDrone.Core.Configuration;
using NzbDrone.Core.MetadataSource.SkyHook.Resource;
using Whisparr.Http.REST;

namespace Whisparr.Api.V3.Config
{
    public class MetadataConfigResource : RestResource
    {
        public TMDbCountryCode CertificationCountry { get; set; }
    }

    public static class MetadataConfigResourceMapper
    {
        public static MetadataConfigResource ToResource(IConfigService model)
        {
            return new MetadataConfigResource
            {
                CertificationCountry = model.CertificationCountry,
            };
        }
    }
}
