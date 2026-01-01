using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NzbDrone.Common.Extensions;

namespace Whisparr.Api.V3.Movies
{
    internal static class MovieFilterEvaluator
    {
        public static IEnumerable<MovieResource> ApplyFilters(IEnumerable<MovieResource> source, IReadOnlyList<MovieFilterDefinition> filters)
        {
            if (filters == null || filters.Count == 0)
            {
                return source;
            }

            foreach (var filter in filters)
            {
                source = source.Where(movie => Matches(movie, filter));
            }

            return source;
        }

        public static IOrderedEnumerable<MovieResource> ApplyOrdering(IEnumerable<MovieResource> source, string sortKey, string sortDirection)
        {
            var key = string.IsNullOrWhiteSpace(sortKey) ? "sortTitle" : sortKey;
            var descending = string.Equals(sortDirection, "descending", StringComparison.OrdinalIgnoreCase);

            return descending
                ? source.OrderByDescending(movie => ResolveSortKey(movie, key))
                : source.OrderBy(movie => ResolveSortKey(movie, key));
        }

        private static IComparable ResolveSortKey(MovieResource movie, string key)
        {
            switch (key)
            {
                case "movieStatus":
                    return BuildMovieStatusSortValue(movie);
                case "status":
                    return BuildStatusSortValue(movie);
                case "studio":
                    return movie.StudioTitle ?? string.Empty;
                case "qualityProfileId":
                    return movie.QualityProfileId;
                case "added":
                    return movie.Added.ToUniversalTime();
                case "releaseDate":
                    return movie.ReleaseDate?.ToUniversalTime() ?? DateTime.MaxValue;
                case "runtime":
                    return movie.Runtime;
                case "path":
                    return movie.Path ?? string.Empty;
                case "sizeOnDisk":
                    return movie.Statistics?.SizeOnDisk ?? 0L;
                case "year":
                    return movie.Year;
                case "tmdbRating":
                    var tmdbValue = movie.Ratings?.Tmdb?.Value ?? 0m;
                    return (double)tmdbValue * 10d;
                case "tmdbVotes":
                    return movie.Ratings?.Tmdb?.Votes ?? 0;
                case "sortTitle":
                    return movie.SortTitle ?? movie.Title ?? string.Empty;
                default:
                    return movie.Title ?? string.Empty;
            }
        }

        private static string BuildStatusSortValue(MovieResource movie)
        {
            var score = 0;

            if (movie.Monitored)
            {
                score += 4;
            }

            var status = movie.Status.ToString().ToLowerInvariant();

            if (status == "announced")
            {
                score += 1;
            }

            if (status == "released")
            {
                score += 3;
            }

            return score.ToString("00", CultureInfo.InvariantCulture) + status;
        }

        private static string BuildMovieStatusSortValue(MovieResource movie)
        {
            var score = 0;
            var qualityName = string.Empty;

            if (movie.IsAvailable)
            {
                score += 1;
            }

            if (movie.Monitored)
            {
                score += 2;
            }

            if (movie.MovieFile != null)
            {
                score += movie.MovieFile.QualityCutoffNotMet ? 4 : 8;
                qualityName = movie.MovieFile.Quality?.Quality?.Name ?? string.Empty;
            }

            return score.ToString("00", CultureInfo.InvariantCulture) + qualityName;
        }

        private static bool Matches(MovieResource movie, MovieFilterDefinition filter)
        {
            var comparison = (filter.Comparison ?? "equal").ToLowerInvariant();
            var requireAll = comparison is "notcontains" or "notequal" or "notstartswith" or "notendswith" or "notinlast" or "notinnext";

            return requireAll
                ? filter.Values.All(value => Evaluate(movie, filter.Key, comparison, value, filter.ValueType))
                : filter.Values.Any(value => Evaluate(movie, filter.Key, comparison, value, filter.ValueType));
        }

        private static bool Evaluate(MovieResource movie, string key, string comparison, object filterValue, string valueType)
        {
            switch (key)
            {
                case "monitored":
                    return EvaluateBoolean(movie.Monitored, comparison, filterValue);
                case "isAvailable":
                    return EvaluateBoolean(movie.IsAvailable, comparison, filterValue);
                case "hasFile":
                    var hasFile = movie.HasFile ?? movie.MovieFileId > 0;
                    return EvaluateBoolean(hasFile, comparison, filterValue);
                case "qualityCutoffNotMet":
                    var cutoff = movie.MovieFile?.QualityCutoffNotMet ?? false;
                    return EvaluateBoolean(cutoff, comparison, filterValue);
                case "status":
                    return EvaluateString(movie.Status.ToString(), comparison, filterValue);
                case "title":
                    return EvaluateString(movie.Title, comparison, filterValue);
                case "sortTitle":
                    return EvaluateString(movie.SortTitle, comparison, filterValue);
                case "studio":
                    return EvaluateString(movie.StudioTitle, comparison, filterValue);
                case "path":
                    return EvaluateString(movie.Path, comparison, filterValue);
                case "genres":
                    return EvaluateCollection(movie.Genres ?? new List<string>(), comparison, filterValue);
                case "itemType":
                    return EvaluateString(movie.ItemType.ToString(), comparison, filterValue);
                case "tags":
                    var tags = movie.Tags ?? new HashSet<int>();
                    return EvaluateCollection(tags.Select(t => t.ToString(CultureInfo.InvariantCulture)), comparison, filterValue);
                case "qualityProfileId":
                    return EvaluateNumeric(movie.QualityProfileId, comparison, filterValue);
                case "sizeOnDisk":
                    var size = movie.Statistics?.SizeOnDisk ?? 0L;
                    return EvaluateNumeric(size, comparison, filterValue);
                case "tmdbRating":
                    var tmdbValue = movie.Ratings?.Tmdb?.Value ?? 0m;
                    var rating = (double)tmdbValue * 10d;
                    return EvaluateNumeric(rating, comparison, filterValue);
                case "tmdbVotes":
                    var votes = movie.Ratings?.Tmdb?.Votes ?? 0;
                    return EvaluateNumeric(votes, comparison, filterValue);
                case "runtime":
                    return EvaluateNumeric(movie.Runtime, comparison, filterValue);
                case "year":
                    return EvaluateNumeric(movie.Year, comparison, filterValue);
                case "releaseDate":
                    return EvaluateDate(movie.ReleaseDate, comparison, filterValue);
                case "added":
                    return EvaluateDate(movie.Added, comparison, filterValue);
                case "releaseGroups":
                    var groups = movie.Statistics?.ReleaseGroups ?? new List<string>();
                    return EvaluateCollection(groups, comparison, filterValue);
                default:
                    return false;
            }
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

        private static bool EvaluateDate(DateTime? itemValue, string comparison, object filterValue)
        {
            if (!itemValue.HasValue)
            {
                return false;
            }

            if (filterValue is DateTime date)
            {
                return comparison switch
                {
                    "lessthan" => itemValue.Value < date,
                    "greaterthan" => itemValue.Value > date,
                    "notequal" => itemValue.Value != date,
                    _ => itemValue.Value == date
                };
            }

            if (filterValue is RelativeDateValue relative)
            {
                var now = DateTime.UtcNow;
                var offset = CalculateOffset(relative);
                var target = itemValue.Value.ToUniversalTime();

                return comparison switch
                {
                    "inlast" => target >= now + offset && target <= now,
                    "notinlast" => target < now + offset,
                    "innext" => target > now && target <= now - offset,
                    "notinnext" => target > now - offset,
                    _ => false
                };
            }

            return false;
        }

        private static TimeSpan CalculateOffset(RelativeDateValue value)
        {
            return value.Unit switch
            {
                "days" => TimeSpan.FromDays(-value.Magnitude),
                "weeks" => TimeSpan.FromDays(-7 * value.Magnitude),
                "months" => TimeSpan.FromDays(-30 * value.Magnitude),
                "years" => TimeSpan.FromDays(-365 * value.Magnitude),
                _ => TimeSpan.FromDays(-value.Magnitude)
            };
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
