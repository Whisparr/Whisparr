using System.Linq;
using Dapper;
using NLog;
using NzbDrone.Common.Cache;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Profiles.Qualities;

namespace NzbDrone.Core.Housekeeping.Housekeepers
{
    public class AssignDefaultQualityProfiles : IHousekeepingTask
    {
        private readonly IMainDatabase _database;
        private readonly IQualityProfileRepository _profileRepository;
        private readonly ICacheManager _cacheManager;
        private readonly Logger _logger;

        public AssignDefaultQualityProfiles(IMainDatabase database, IQualityProfileRepository profileRepository, ICacheManager cacheManager, Logger logger)
        {
            _database = database;
            _profileRepository = profileRepository;
            _cacheManager = cacheManager;
            _logger = logger;
        }

        public void Clean()
        {
            var profiles = _profileRepository.All().ToList();

            if (!profiles.Any())
            {
                return;  // Nothing to assign if there are no profiles
            }

            var defaultProfileId = profiles.First().Id;
            var validIds = profiles.Select(p => p.Id).ToArray();

            using var mapper = _database.OpenConnection();

            // Define cache targets for revocation
            var targets = new (string Table, string CacheName, string Label)[]
            {
                ("Movies", "Whisparr.Api.V3.Movies.MovieResource_movieResources", "movie"),
                ("Performers", "Whisparr.Api.V3.Performers.PerformerResource_performerResources", "performer"),
                ("Studios", "Whisparr.Api.V3.Studios.StudioResource_studioResources", "studio")
            };

            // Track overall results
            var totalAffected = 0;
            var totalRevoked = 0;

            var impactedIdsWhere = "WHERE \"QualityProfileId\" IS NULL OR \"QualityProfileId\" NOT IN @ValidIds";
            var updateWhere = "WHERE \"QualityProfileId\" IS NULL OR \"QualityProfileId\" NOT IN @ValidIds";

            if (_database.DatabaseType == DatabaseType.PostgreSQL)
            {
                impactedIdsWhere = "WHERE \"QualityProfileId\" IS NULL OR \"QualityProfileId\" <> ALL(@ValidIds::int[])";
                updateWhere = "WHERE \"QualityProfileId\" IS NULL OR \"QualityProfileId\" <> ALL(@ValidIds::int[])";
            }

            foreach (var t in targets)
            {
                var impactedIds = mapper.Query<int>($@"SELECT ""Id"" FROM ""{t.Table}"" {impactedIdsWhere}",
                    new { ValidIds = validIds }).ToList();

                if (!impactedIds.Any())
                {
                    continue;
                }

                // Update quality profiles
                var sql = $@"UPDATE ""{t.Table}""
                      SET ""QualityProfileId"" = @DefaultProfileId
                      {updateWhere}";

                var affected = mapper.Execute(sql, new { DefaultProfileId = defaultProfileId, ValidIds = validIds });
                totalAffected += affected;

                var revokedForThis = 0;
                var resourcesCache = _cacheManager.FindCache(t.CacheName);

                if (resourcesCache != null)
                {
                    foreach (var id in impactedIds)
                    {
                        // Invalidate API cache entries for impacted items
                        resourcesCache.Remove(id.ToString());
                    }

                    revokedForThis = impactedIds.Count;
                    totalRevoked += revokedForThis;
                }

                // Log the per-table results
                _logger.Info("Assigned default quality profile [{0}] to [{1}] {2}(s) and revoked cache for [{3}] {2}(s)", defaultProfileId, affected, t.Label, revokedForThis);
            }

            _logger.Info("AssignDefaultQualityProfiles completed: totalAffected={0}, totalCacheRevoked={1}", totalAffected, totalRevoked);
        }
    }
}
