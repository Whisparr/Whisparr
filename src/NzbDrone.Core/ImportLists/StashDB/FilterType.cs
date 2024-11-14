using System.Collections.Generic;
using Newtonsoft.Json;

namespace NzbDrone.Core.ImportLists.StashDB
{
    public class FilterType
    {
        [JsonProperty("modifier")]
        public FilterModifier Modifier { get; set; }
        [JsonProperty("value")]
        public List<string> Value { get; set; }

        public FilterType(FilterModifier filterModifier, List<string> value)
        {
            Modifier = filterModifier;
            Value = value;
        }
    }
}
