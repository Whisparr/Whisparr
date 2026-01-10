using NzbDrone.Core.Movies.AlternativeTitles;

namespace NzbDrone.Core.MetadataSource.SkyHook.Resource
{
    public class AlternativeTitleResource
    {
        public string Title { get; set; }
        public SourceType Type { get; set; }
        public string Language { get; set; }
    }
}
