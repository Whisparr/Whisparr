using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace NzbDrone.Core.ImportLists.StashDB
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum FavoriteFilter
    {
        ALL,
        PERFORMER,
        STUDIO
    }
}
