using System;
using Newtonsoft.Json;

namespace NzbDrone.Core.ImportLists.StashDB
{
    [JsonConverter(typeof(SceneSortConverter))]
    public enum SceneSort
    {
        RELEASED,
        CREATED,
        TRENDING
    }

    public class SceneSortConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(SceneSort);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var sort = (SceneSort)value;
            var sortString = sort switch
            {
                SceneSort.RELEASED => "DATE",
                SceneSort.CREATED => "CREATED_AT",
                SceneSort.TRENDING => "TRENDING",
                _ => throw new ArgumentOutOfRangeException(nameof(sort), sort, null)
            };

            writer.WriteValue(sortString);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var sortString = (string)reader.Value;
            return sortString switch
            {
                "DATE" => SceneSort.RELEASED,
                "CREATED_AT" => SceneSort.CREATED,
                "TRENDING" => SceneSort.TRENDING,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}
