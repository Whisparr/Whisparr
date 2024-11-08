using System.Collections.Generic;
using Newtonsoft.Json;

namespace NzbDrone.Core.ImportLists.StashDB
{
    public class FilterType
    {
        [JsonProperty("modifier")]
        public string Modifier { get; set; }
        [JsonProperty("value")]
        public List<string> Value { get; set; }

        public FilterType(List<string> value)
        {
            Modifier = "INCLUDES";
            Value = value;
        }
    }
}
