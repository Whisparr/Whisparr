using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NzbDrone.Common.Extensions;

namespace Whisparr.Api.V3.Studios
{
    internal static class StudioFilterEvaluator
    {
        public static IEnumerable<StudioResource> ApplyFilters(IEnumerable<StudioResource> source, IReadOnlyList<StudioFilterDefinition> filters)
        {
            if (filters == null || filters.Count == 0)
            {
                return source;
            }

            foreach (var filter in filters)
            {
                source = source.Where(studio => Matches(studio, filter));
            }

            return source;
        }

        public static IOrderedEnumerable<StudioResource> ApplyOrdering(IEnumerable<StudioResource> source, string sortKey, string sortDirection)
        {
            var key = string.IsNullOrWhiteSpace(sortKey) ? "sortTitle" : sortKey;
            var descending = string.Equals(sortDirection, "descending", StringComparison.OrdinalIgnoreCase);

            return descending
                ? source.OrderByDescending(studio => ResolveSortKey(studio, key))
                : source.OrderBy(studio => ResolveSortKey(studio, key));
        }

        private static IComparable ResolveSortKey(StudioResource studio, string key)
        {
            switch (key)
            {
                case "status":
                    return studio.Status.ToString();
                case "network":
                    return studio.Network ?? string.Empty;
                case "qualityProfileId":
                    return studio.QualityProfileId;
                case "rootFolderPath":
                    return studio.RootFolderPath ?? string.Empty;
                case "sceneCount":
                    return studio.SceneCount;
                case "totalSceneCount":
                    return studio.TotalSceneCount;
                case "sizeOnDisk":
                    return studio.SizeOnDisk;
                case "title":
                case "sortTitle":
                    return studio.SortTitle ?? studio.Title ?? string.Empty;
                default:
                    return studio.Title ?? string.Empty;
            }
        }

        private static bool Matches(StudioResource studio, StudioFilterDefinition filter)
        {
            var comparison = (filter.Comparison ?? "equal").ToLowerInvariant();
            var requireAll = comparison is "notcontains" or "notequal" or "notstartswith" or "notendswith";

            return requireAll
                ? filter.Values.All(value => Evaluate(studio, filter.Key, comparison, value, filter.ValueType))
                : filter.Values.Any(value => Evaluate(studio, filter.Key, comparison, value, filter.ValueType));
        }

        private static bool Evaluate(StudioResource studio, string key, string comparison, object filterValue, string valueType)
        {
            switch (key)
            {
                case "monitored":
                    return EvaluateBoolean(studio.Monitored, comparison, filterValue);
                case "moviesMonitored":
                    return EvaluateBoolean(studio.MoviesMonitored, comparison, filterValue);
                case "qualityProfileId":
                    return EvaluateNumeric(studio.QualityProfileId, comparison, filterValue);
                case "title":
                    return EvaluateString(studio.Title, comparison, filterValue);
                case "sortTitle":
                    return EvaluateString(studio.SortTitle, comparison, filterValue);
                case "status":
                    return EvaluateString(studio.Status.ToString(), comparison, filterValue);
                case "sceneCount":
                    return EvaluateNumeric(studio.SceneCount, comparison, filterValue);
                case "totalSceneCount":
                    return EvaluateNumeric(studio.TotalSceneCount, comparison, filterValue);
                case "network":
                    return EvaluateString(studio.Network, comparison, filterValue);
                case "rootFolderPath":
                    return EvaluateString(studio.RootFolderPath, comparison, filterValue);
                case "monitor":
                    return EvaluateMonitorOption(studio, comparison, filterValue);
                case "tags":
                    var tags = studio.Tags ?? new HashSet<int>();
                    return EvaluateCollection(tags.Select(tag => tag.ToString(CultureInfo.InvariantCulture)), comparison, filterValue);
                default:
                    return false;
            }
        }

        private static bool EvaluateMonitorOption(StudioResource studio, string comparison, object filterValue)
        {
            var value = filterValue?.ToString();

            if (value.IsNullOrWhiteSpace())
            {
                return false;
            }

            var normalized = value.ToLowerInvariant();
            var matches = normalized switch
            {
                "all" => studio.Monitored && studio.MoviesMonitored,
                "movieonly" => !studio.Monitored && studio.MoviesMonitored,
                "none" => !studio.Monitored && !studio.MoviesMonitored,
                _ => false
            };

            return comparison switch
            {
                "notequal" => !matches,
                _ => matches
            };
        }

        private static bool EvaluateBoolean(bool itemValue, string comparison, object filterValue)
        {
            if (!TryCoerceBoolean(filterValue, out var desired))
            {
                return false;
            }

            return comparison switch
            {
                "notequal" => itemValue != desired,
                _ => itemValue == desired
            };
        }

        private static bool TryCoerceBoolean(object value, out bool result)
        {
            switch (value)
            {
                case bool boolean:
                    result = boolean;
                    return true;
                case double numeric:
                    result = Math.Abs(numeric) > double.Epsilon;
                    return true;
                case string text when bool.TryParse(text, out var parsed):
                    result = parsed;
                    return true;
                default:
                    result = false;
                    return false;
            }
        }

        private static bool EvaluateString(string itemValue, string comparison, object filterValue)
        {
            if (itemValue.IsNullOrWhiteSpace() || filterValue == null)
            {
                return false;
            }

            var candidate = filterValue.ToString();

            if (candidate.IsNullOrWhiteSpace())
            {
                return false;
            }

            var value = itemValue.ToLowerInvariant();
            var filter = candidate.ToLowerInvariant();

            return comparison switch
            {
                "contains" => value.Contains(filter),
                "notcontains" => !value.Contains(filter),
                "startswith" => value.StartsWith(filter, StringComparison.Ordinal),
                "notstartswith" => !value.StartsWith(filter, StringComparison.Ordinal),
                "endswith" => value.EndsWith(filter, StringComparison.Ordinal),
                "notendswith" => !value.EndsWith(filter, StringComparison.Ordinal),
                "notequal" => !value.Equals(filter, StringComparison.Ordinal),
                _ => value.Equals(filter, StringComparison.Ordinal)
            };
        }

        private static bool EvaluateNumeric(double itemValue, string comparison, object filterValue)
        {
            if (!TryCoerceNumeric(filterValue, out var desired))
            {
                return false;
            }

            return comparison switch
            {
                "greaterthan" => itemValue > desired,
                "greaterthanorequal" => itemValue >= desired,
                "lessthan" => itemValue < desired,
                "lessthanorequal" => itemValue <= desired,
                "notequal" => Math.Abs(itemValue - desired) > double.Epsilon,
                _ => Math.Abs(itemValue - desired) < double.Epsilon
            };
        }

        private static bool TryCoerceNumeric(object value, out double result)
        {
            switch (value)
            {
                case double numeric:
                    result = numeric;
                    return true;
                case int integer:
                    result = integer;
                    return true;
                case long longValue:
                    result = longValue;
                    return true;
                case string text when double.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed):
                    result = parsed;
                    return true;
                default:
                    result = 0;
                    return false;
            }
        }

        private static bool EvaluateCollection(IEnumerable<string> items, string comparison, object filterValue)
        {
            var collection = items?.Where(item => !item.IsNullOrWhiteSpace()).Select(item => item.ToLowerInvariant()).ToList() ?? new List<string>();

            var candidate = filterValue switch
            {
                string text => text.ToLowerInvariant(),
                double numeric => numeric.ToString(CultureInfo.InvariantCulture).ToLowerInvariant(),
                int integer => integer.ToString(CultureInfo.InvariantCulture).ToLowerInvariant(),
                _ => filterValue?.ToString()?.ToLowerInvariant()
            };

            if (candidate.IsNullOrWhiteSpace())
            {
                return false;
            }

            return comparison switch
            {
                "notcontains" => !collection.Contains(candidate),
                "notequal" => !collection.Contains(candidate),
                _ => collection.Contains(candidate)
            };
        }
    }
}
