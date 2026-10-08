using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using FluentValidation.Results;
using Newtonsoft.Json;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Movies.Performers;

namespace NzbDrone.Core.Notifications.Stash
{
    public interface IStashService
    {
        void GetStatus(StashSettings settings);
        void Clean(StashSettings settings, Movie movie);
        void Update(StashSettings settings, Movie movie);
        StashPerformerSyncResult SyncPerformers(StashSettings settings, Action checkpoint = null);
        ValidationFailure Test(StashSettings settings);
    }

    public class StashService : IStashService
    {
        private readonly IStashProxy _proxy;
        private readonly IPerformerService _performerService;
        private readonly IAddPerformerService _addPerformerService;
        private readonly Logger _logger;

        public StashService(IStashProxy proxy,
                            IPerformerService performerService,
                            IAddPerformerService addPerformerService,
                            Logger logger)
        {
            _proxy = proxy;
            _performerService = performerService;
            _addPerformerService = addPerformerService;
            _logger = logger;
        }

        public void Clean(StashSettings settings, Movie movie)
        {
            var seriesLocation = new OsPath(movie.Path);
            var mappedPath = seriesLocation;

            if (settings.MapTo.IsNotNullOrWhiteSpace())
            {
                mappedPath = new OsPath(settings.MapTo) + (seriesLocation - new OsPath(settings.MapFrom));

                _logger.Trace("Mapping Path from {0} to {1} for partial scan", seriesLocation, mappedPath);
            }

            _proxy.Clean(settings, mappedPath.FullPath);
        }

        public void Update(StashSettings settings, Movie movie)
        {
            var seriesLocation = new OsPath(movie.Path);
            var mappedPath = seriesLocation;

            if (settings.MapTo.IsNotNullOrWhiteSpace())
            {
                mappedPath = new OsPath(settings.MapTo) + (seriesLocation - new OsPath(settings.MapFrom));

                _logger.Trace("Mapping Path from {0} to {1} for partial scan", seriesLocation, mappedPath);
            }

            _proxy.Update(settings, mappedPath.FullPath);
        }

        public void GetStatus(StashSettings settings)
        {
            _proxy.GetStatus(settings);
        }

        public StashPerformerSyncResult SyncPerformers(StashSettings settings, Action checkpoint = null)
        {
            var result = new StashPerformerSyncResult();

            if (settings.PerformerSyncMode == StashPerformerSyncMode.Disabled)
            {
                return result;
            }

            // Both inventories must be read successfully before either application is changed.
            // This keeps an outage or partial response from looking like a mass removal.
            var whisparrPerformers = _performerService.GetAllPerformers();
            var stashPerformers = _proxy.GetPerformers(settings);

            var syncState = ReadSyncState(settings.PerformerSyncState);

            var whisparrByForeignId = BuildWhisparrMap(whisparrPerformers, result);
            var stashByForeignId = BuildStashMap(stashPerformers, result);
            var foreignIds = new HashSet<string>(syncState.Keys, StringComparer.OrdinalIgnoreCase);
            foreignIds.UnionWith(whisparrByForeignId.Keys);
            foreignIds.UnionWith(stashByForeignId.Keys);

            foreach (var foreignId in foreignIds.OrderBy(id => id, StringComparer.OrdinalIgnoreCase))
            {
                result.Checked++;
                whisparrByForeignId.TryGetValue(foreignId, out var whisparrPerformer);
                stashByForeignId.TryGetValue(foreignId, out var stashMatches);

                if (stashMatches?.Count > 1)
                {
                    AddWarning(result, $"Skipping StashDB performer {foreignId}: multiple local Stash performers have the same identity");
                    continue;
                }

                var stashPerformer = stashMatches?.SingleOrDefault();
                var stashFavorite = stashPerformer?.Favorite == true;
                var whisparrMonitored = whisparrPerformer?.Monitored == true;
                syncState.TryGetValue(foreignId, out var previous);

                if (previous == null && !stashFavorite && !whisparrMonitored)
                {
                    continue;
                }

                var target = ResolveTarget(settings.PerformerSyncMode, previous, stashFavorite, whisparrMonitored);

                try
                {
                    if (SyncsToWhisparr(settings.PerformerSyncMode))
                    {
                        if (target && whisparrPerformer == null)
                        {
                            if (stashPerformer == null)
                            {
                                throw new InvalidOperationException("Cannot create a Whisparr performer without a matching Stash performer");
                            }

                            whisparrPerformer = _addPerformerService.AddPerformer(new Performer
                            {
                                ForeignId = foreignId,
                                Name = stashPerformer.Name,
                                Monitored = true,
                                MoviesMonitored = false,
                                RootFolderPath = settings.PerformerSyncRootFolderPath,
                                QualityProfileId = settings.PerformerSyncQualityProfileId,
                                SearchOnAdd = false,
                                Tags = new HashSet<int>()
                            });

                            if (whisparrPerformer?.Monitored != true)
                            {
                                throw new InvalidOperationException("Whisparr did not persist monitoring for the created performer");
                            }

                            whisparrMonitored = true;
                            result.Created++;
                        }
                        else if (whisparrPerformer != null && whisparrMonitored != target)
                        {
                            whisparrPerformer.Monitored = target;
                            _performerService.Update(whisparrPerformer);
                            whisparrMonitored = target;
                            result.Updated++;
                        }
                    }

                    if (SyncsToStash(settings.PerformerSyncMode))
                    {
                        if (target && stashPerformer == null)
                        {
                            if (whisparrPerformer == null)
                            {
                                throw new InvalidOperationException("Cannot create a Stash performer without a matching Whisparr performer");
                            }

                            stashPerformer = _proxy.CreatePerformer(settings, whisparrPerformer.Name, foreignId);

                            if (stashPerformer?.Favorite != true)
                            {
                                throw new InvalidOperationException("Stash did not persist favorite state for the created performer");
                            }

                            stashFavorite = true;
                            result.Created++;
                        }
                        else if (stashPerformer != null && stashFavorite != target)
                        {
                            stashPerformer = _proxy.UpdatePerformerFavorite(settings, stashPerformer.Id, target);

                            if (stashPerformer.Favorite != target)
                            {
                                throw new InvalidOperationException("Stash did not persist the requested favorite state");
                            }

                            stashFavorite = stashPerformer.Favorite;
                            result.Updated++;
                        }
                    }

                    var current = new StashPerformerSyncState
                    {
                        StashFavorite = stashFavorite,
                        WhisparrMonitored = whisparrMonitored,
                        StashId = stashPerformer?.Id,
                        WhisparrId = whisparrPerformer?.Id ?? 0
                    };

                    if (!StateEquals(previous, current))
                    {
                        syncState[foreignId] = current;
                        settings.PerformerSyncState = JsonConvert.SerializeObject(syncState);
                        checkpoint?.Invoke();
                    }
                }
                catch (Exception ex)
                {
                    AddWarning(result, $"Failed to synchronize StashDB performer {foreignId}: {ex.Message}");
                }
            }

            _logger.Info("Stash performer sync checked {0} identities, created {1}, updated {2}, warnings {3}", result.Checked, result.Created, result.Updated, result.Warnings);
            return result;
        }

        internal static bool ResolveTarget(StashPerformerSyncMode mode, StashPerformerSyncState previous, bool stashFavorite, bool whisparrMonitored)
        {
            if (mode == StashPerformerSyncMode.WhisparrToStash)
            {
                return whisparrMonitored;
            }

            if (mode == StashPerformerSyncMode.StashToWhisparr)
            {
                return stashFavorite;
            }

            if (previous == null)
            {
                return stashFavorite || whisparrMonitored;
            }

            var stashChanged = stashFavorite != previous.StashFavorite;
            var whisparrChanged = whisparrMonitored != previous.WhisparrMonitored;

            if (stashChanged && whisparrChanged)
            {
                return stashFavorite == whisparrMonitored ? stashFavorite : false;
            }

            if (stashChanged)
            {
                return stashFavorite;
            }

            if (whisparrChanged)
            {
                return whisparrMonitored;
            }

            return stashFavorite == whisparrMonitored ? stashFavorite : stashFavorite || whisparrMonitored;
        }

        private Dictionary<string, Performer> BuildWhisparrMap(IEnumerable<Performer> performers, StashPerformerSyncResult result)
        {
            var map = new Dictionary<string, Performer>(StringComparer.OrdinalIgnoreCase);

            foreach (var group in performers.Where(performer => performer.ForeignId.IsNotNullOrWhiteSpace()).GroupBy(performer => performer.ForeignId, StringComparer.OrdinalIgnoreCase))
            {
                if (group.Count() != 1)
                {
                    AddWarning(result, $"Skipping StashDB performer {group.Key}: multiple Whisparr performers have the same identity");
                    continue;
                }

                map[group.Key] = group.Single();
            }

            return map;
        }

        private Dictionary<string, List<StashPerformer>> BuildStashMap(IEnumerable<StashPerformer> performers, StashPerformerSyncResult result)
        {
            var map = new Dictionary<string, List<StashPerformer>>(StringComparer.OrdinalIgnoreCase);

            foreach (var performer in performers)
            {
                var ids = (performer.StashIds ?? new List<StashPerformerId>())
                    .Where(id => StashProxy.IsStashDbEndpoint(id.Endpoint) && id.StashId.IsNotNullOrWhiteSpace())
                    .Select(id => id.StashId)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                if (ids.Count != 1)
                {
                    if (performer.Favorite)
                    {
                        AddWarning(result, $"Skipping favorite Stash performer {performer.Id}: missing or ambiguous StashDB identity");
                    }

                    continue;
                }

                if (!map.TryGetValue(ids[0], out var matches))
                {
                    matches = new List<StashPerformer>();
                    map[ids[0]] = matches;
                }

                matches.Add(performer);
            }

            return map;
        }

        private static bool SyncsToWhisparr(StashPerformerSyncMode mode)
        {
            return mode == StashPerformerSyncMode.StashToWhisparr || mode == StashPerformerSyncMode.Bidirectional;
        }

        private static bool SyncsToStash(StashPerformerSyncMode mode)
        {
            return mode == StashPerformerSyncMode.WhisparrToStash || mode == StashPerformerSyncMode.Bidirectional;
        }

        private static bool StateEquals(StashPerformerSyncState left, StashPerformerSyncState right)
        {
            return left != null &&
                   left.StashFavorite == right.StashFavorite &&
                   left.WhisparrMonitored == right.WhisparrMonitored &&
                   left.StashId == right.StashId &&
                   left.WhisparrId == right.WhisparrId;
        }

        private static Dictionary<string, StashPerformerSyncState> ReadSyncState(string value)
        {
            if (value.IsNullOrWhiteSpace())
            {
                return new Dictionary<string, StashPerformerSyncState>(StringComparer.OrdinalIgnoreCase);
            }

            try
            {
                var state = JsonConvert.DeserializeObject<Dictionary<string, StashPerformerSyncState>>(value);
                return new Dictionary<string, StashPerformerSyncState>(state ?? new Dictionary<string, StashPerformerSyncState>(), StringComparer.OrdinalIgnoreCase);
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException("The saved Stash performer sync state is invalid", ex);
            }
        }

        private void AddWarning(StashPerformerSyncResult result, string message)
        {
            result.Warnings++;
            result.WarningMessages.Add(message);
            _logger.Warn(message);
        }

        public ValidationFailure Test(StashSettings settings)
        {
            try
            {
                _logger.Debug("Testing connection to Stash: {0}", settings.Address);

                GetStatus(settings);
            }
            catch (HttpException ex)
            {
                if (ex.Response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    return new ValidationFailure("ApiKey", "API Key is incorrect");
                }

                return new ValidationFailure("Host", "Unable to send test message: " + ex.Message);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Unable to send test message");
                return new ValidationFailure("Host", "Unable to send test message: " + ex.Message);
            }

            return null;
        }
    }
}
