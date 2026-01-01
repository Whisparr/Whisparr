using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NzbDrone.Common.Extensions;

namespace Whisparr.Api.V3.Performers
{
    internal static class PerformerFilterEvaluator
    {
        public static IEnumerable<PerformerResource> ApplyFilters(IEnumerable<PerformerResource> source, IReadOnlyList<PerformerFilterDefinition> filters)
        {
            if (filters == null || filters.Count == 0)
            {
                return source;
            }

            foreach (var filter in filters)
            {
                source = source.Where(performer => Matches(performer, filter));
            }

            return source;
        }

        public static IOrderedEnumerable<PerformerResource> ApplyOrdering(IEnumerable<PerformerResource> source, string sortKey, string sortDirection)
        {
            var key = string.IsNullOrWhiteSpace(sortKey) ? "fullName" : sortKey;
            var descending = string.Equals(sortDirection, "descending", StringComparison.OrdinalIgnoreCase);

            return descending
                ? source.OrderByDescending(performer => ResolveSortKey(performer, key))
                : source.OrderBy(performer => ResolveSortKey(performer, key));
        }

        private static IComparable ResolveSortKey(PerformerResource performer, string key)
        {
            switch (key)
            {
                case "status":
                    return performer.Status.ToString();
                case "gender":
                    return performer.Gender.ToString();
                case "hairColor":
                    return performer.HairColor?.ToString() ?? string.Empty;
                case "ethnicity":
                    return performer.Ethnicity?.ToString() ?? string.Empty;
                case "qualityProfileId":
                    return performer.QualityProfileId;
                case "rootFolderPath":
                    return performer.RootFolderPath ?? string.Empty;
                case "sceneCount":
                    return performer.SceneCount;
                case "totalSceneCount":
                    return performer.TotalSceneCount;
                case "age":
                    return performer.Age ?? -1;
                case "careerStart":
                    return performer.CareerStart ?? int.MinValue;
                case "careerEnd":
                    return performer.CareerEnd ?? int.MaxValue;
                case "sizeOnDisk":
                    return performer.SizeOnDisk;
                case "added":
                    return performer.Added;
                case "fullName":
                    return performer.FullName ?? string.Empty;
                default:
                    return performer.FullName ?? string.Empty;
            }
        }

        private static bool Matches(PerformerResource performer, PerformerFilterDefinition filter)
        {
            var comparison = (filter.Comparison ?? "equal").ToLowerInvariant();
            var requireAll = comparison is "notcontains" or "notequal" or "notstartswith" or "notendswith";

            return requireAll
                ? filter.Values.All(value => Evaluate(performer, filter.Key, comparison, value, filter.ValueType))
                : filter.Values.Any(value => Evaluate(performer, filter.Key, comparison, value, filter.ValueType));
        }

        private static bool Evaluate(PerformerResource performer, string key, string comparison, object filterValue, string valueType)
        {
            switch (key)
            {
                case "monitored":
                    return EvaluateBoolean(performer.Monitored, comparison, filterValue);
                case "moviesMonitored":
                    return EvaluateBoolean(performer.MoviesMonitored, comparison, filterValue);
                case "sceneCount":
                    return EvaluateNumeric(performer.SceneCount, comparison, filterValue);
                case "totalSceneCount":
                    return EvaluateNumeric(performer.TotalSceneCount, comparison, filterValue);
                case "age":
                    return EvaluateNumeric(performer.Age ?? 0, comparison, filterValue);
                case "careerStart":
                    return EvaluateNumeric(performer.CareerStart ?? 0, comparison, filterValue);
                case "careerEnd":
                    return EvaluateNumeric(performer.CareerEnd ?? 0, comparison, filterValue);
                case "status":
                    return EvaluateString(performer.Status.ToString(), comparison, filterValue);
                case "fullName":
                    return EvaluateString(performer.FullName, comparison, filterValue);
                case "rootFolderPath":
                    return EvaluateString(performer.RootFolderPath, comparison, filterValue);
                case "monitor":
                    return EvaluateMonitorOption(performer, comparison, filterValue);
                case "qualityProfileId":
                    return EvaluateNumeric(performer.QualityProfileId, comparison, filterValue);
                case "gender":
                    return EvaluateString(performer.Gender.ToString(), comparison, filterValue);
                case "hairColor":
                    return EvaluateString(performer.HairColor?.ToString(), comparison, filterValue);
                case "ethnicity":
                    return EvaluateString(performer.Ethnicity?.ToString(), comparison, filterValue);
                case "tags":
                    var tags = performer.Tags ?? new HashSet<int>();
                    return EvaluateCollection(tags.Select(tag => tag.ToString(CultureInfo.InvariantCulture)), comparison, filterValue);
                default:
                    return false;
            }
        }

        private static bool EvaluateMonitorOption(PerformerResource performer, string comparison, object filterValue)
        {
            var value = filterValue?.ToString();

            if (value.IsNullOrWhiteSpace())
            {
                return false;
            }

            var normalized = value.ToLowerInvariant();
            var matches = normalized switch
            {
                "all" => performer.Monitored && performer.MoviesMonitored,
                "movieonly" => !performer.Monitored && performer.MoviesMonitored,
                "none" => !performer.Monitored && !performer.MoviesMonitored,
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
