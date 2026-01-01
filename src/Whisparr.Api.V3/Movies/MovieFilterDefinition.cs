using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;

namespace Whisparr.Api.V3.Movies
{
    internal sealed class MovieFilterDefinition
    {
        public string Key { get; init; }

        public string Comparison { get; init; }

        public string ValueType { get; init; }

        public IReadOnlyList<object> Values { get; init; }

        public static IReadOnlyList<MovieFilterDefinition> Parse(string payload)
        {
            if (string.IsNullOrWhiteSpace(payload))
            {
                return Array.Empty<MovieFilterDefinition>();
            }

            try
            {
                using var json = JsonDocument.Parse(payload);

                if (json.RootElement.ValueKind != JsonValueKind.Array)
                {
                    return Array.Empty<MovieFilterDefinition>();
                }

                var filters = new List<MovieFilterDefinition>();

                foreach (var element in json.RootElement.EnumerateArray())
                {
                    if (!element.TryGetProperty("key", out var keyProperty) || keyProperty.ValueKind != JsonValueKind.String)
                    {
                        continue;
                    }

                    var comparison = element.TryGetProperty("type", out var typeProperty) && typeProperty.ValueKind == JsonValueKind.String
                        ? typeProperty.GetString()
                        : "equal";

                    var valueType = element.TryGetProperty("valueType", out var valueTypeProperty) && valueTypeProperty.ValueKind == JsonValueKind.String
                        ? valueTypeProperty.GetString()
                        : null;

                    if (!element.TryGetProperty("value", out var valueProperty) || valueProperty.ValueKind == JsonValueKind.Null)
                    {
                        continue;
                    }

                    var values = ExtractValues(valueProperty, valueType);

                    if (values.Count == 0)
                    {
                        continue;
                    }

                    filters.Add(new MovieFilterDefinition
                    {
                        Key = keyProperty.GetString(),
                        Comparison = comparison,
                        ValueType = valueType,
                        Values = values
                    });
                }

                return filters;
            }
            catch (JsonException)
            {
                return Array.Empty<MovieFilterDefinition>();
            }
        }

        private static IReadOnlyList<object> ExtractValues(JsonElement valueElement, string valueType)
        {
            if (valueElement.ValueKind == JsonValueKind.Array)
            {
                var items = new List<object>();

                foreach (var item in valueElement.EnumerateArray())
                {
                    if (TryConvert(item, valueType, out var converted))
                    {
                        items.Add(converted);
                    }
                }

                return items;
            }

            if (TryConvert(valueElement, valueType, out var single))
            {
                return new[] { single };
            }

            return Array.Empty<object>();
        }

        private static bool TryConvert(JsonElement element, string valueType, out object result)
        {
            result = null;

            switch (valueType)
            {
                case "bool":
                    if (element.ValueKind == JsonValueKind.True || element.ValueKind == JsonValueKind.False)
                    {
                        result = element.GetBoolean();
                        return true;
                    }

                    if (element.ValueKind == JsonValueKind.String && bool.TryParse(element.GetString(), out var boolValue))
                    {
                        result = boolValue;
                        return true;
                    }

                    return false;

                case "bytes":
                case "number":
                case "qualityProfile":
                case "tag":
                case "language":
                case "quality":
                case "protocol":
                case "queueStatus":
                case "movie":
                case "historyEventType":
                    if (element.ValueKind == JsonValueKind.Number)
                    {
                        result = element.GetDouble();
                        return true;
                    }

                    if (element.ValueKind == JsonValueKind.String && double.TryParse(element.GetString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var numeric))
                    {
                        result = numeric;
                        return true;
                    }

                    return false;

                case "date":
                    if (element.ValueKind == JsonValueKind.Object)
                    {
                        if (element.TryGetProperty("value", out var magnitudeProperty) && magnitudeProperty.TryGetInt32(out var magnitude) &&
                            element.TryGetProperty("time", out var unitProperty) && unitProperty.ValueKind == JsonValueKind.String)
                        {
                            result = new RelativeDateValue(magnitude, unitProperty.GetString());
                            return true;
                        }

                        return false;
                    }

                    if (element.ValueKind == JsonValueKind.String && DateTime.TryParse(element.GetString(), CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var date))
                    {
                        result = date;
                        return true;
                    }

                    return false;

                default:
                    switch (element.ValueKind)
                    {
                        case JsonValueKind.String:
                            result = element.GetString();
                            return !string.IsNullOrWhiteSpace((string)result);
                        case JsonValueKind.Number:
                            result = element.GetDouble();
                            return true;
                        case JsonValueKind.True:
                            result = true;
                            return true;
                        case JsonValueKind.False:
                            result = false;
                            return true;
                        default:
                            return false;
                    }
            }
        }
    }

    internal sealed class RelativeDateValue
    {
        public RelativeDateValue(int magnitude, string unit)
        {
            Magnitude = magnitude;
            Unit = unit;
        }

        public int Magnitude { get; }

        public string Unit { get; }
    }
}
