using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;
using FluentMigrator;
using Newtonsoft.Json;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(217)]
    public class add_size_to_quality_profiles : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Execute.WithConnection(ConvertProfile);
            Delete.Column("MinSize").FromTable("QualityDefinitions");
            Delete.Column("MaxSize").FromTable("QualityDefinitions");
            Delete.Column("PreferredSize").FromTable("QualityDefinitions");
        }

        private void ConvertProfile(IDbConnection conn, IDbTransaction tran)
        {
            var updater = new ProfileUpdater217(conn, tran);

            updater.SetSizes();
            updater.Commit();
        }
    }

    public class Definition217
    {
        public double? MinSize { get; set; }
        public double? MaxSize { get; set; }
        public double? PreferredSize { get; set; }
    }

    public class Profile217
    {
        public int Id { get; set; }
        public List<ProfileItem217> Items { get; set; }
    }

    public class ProfileItem217
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int Id { get; set; }

        public string Name { get; set; }
        public int? Quality { get; set; }
        public List<ProfileItem217> Items { get; set; }
        public bool Allowed { get; set; }
        public double? MinSize { get; set; }
        public double? MaxSize { get; set; }
        public double? PreferredSize { get; set; }
    }

    public class ProfileUpdater217
    {
        private readonly IDbConnection _connection;
        private readonly IDbTransaction _transaction;

        private List<Profile217> _profiles;
        private Dictionary<int, Definition217> _sizes;
        private HashSet<Profile217> _changedProfiles = new HashSet<Profile217>();

        public ProfileUpdater217(IDbConnection conn, IDbTransaction tran)
        {
            _connection = conn;
            _transaction = tran;

            _profiles = GetProfiles();
            _sizes = GetSizes();
        }

        public void Commit()
        {
            var profilesToUpdate = _changedProfiles.Select(p => new
            {
                Id = p.Id,
                Items = p.Items.ToJson()
            });

            var updateSql = $"UPDATE \"QualityProfiles\" SET \"Items\" = @Items WHERE \"Id\" = @Id";
            _connection.Execute(updateSql, profilesToUpdate, transaction: _transaction);

            _changedProfiles.Clear();
        }

        public void SetSizes()
        {
            foreach (var profile in _profiles)
            {
                foreach (var item in profile.Items)
                {
                    if (item.Quality.HasValue)
                    {
                        if (_sizes.TryGetValue(item.Quality.Value, out var sizes))
                        {
                            item.MinSize = sizes.MinSize;
                            item.MaxSize = sizes.MaxSize;
                            item.PreferredSize = sizes.PreferredSize;
                        }
                    }

                    foreach (var groupedItem in item.Items)
                    {
                        if (groupedItem.Quality.HasValue)
                        {
                            if (_sizes.TryGetValue(groupedItem.Quality.Value, out var sizes))
                            {
                                groupedItem.MinSize = sizes.MinSize;
                                groupedItem.MaxSize = sizes.MaxSize;
                                groupedItem.PreferredSize = sizes.PreferredSize;
                            }
                        }
                    }
                }

                _changedProfiles.Add(profile);
            }
        }

        private List<Profile217> GetProfiles()
        {
            var profiles = new List<Profile217>();

            using (var getProfilesCmd = _connection.CreateCommand())
            {
                getProfilesCmd.Transaction = _transaction;
                getProfilesCmd.CommandText = "SELECT \"Id\", \"Items\" FROM \"QualityProfiles\"";

                using (var profileReader = getProfilesCmd.ExecuteReader())
                {
                    while (profileReader.Read())
                    {
                        profiles.Add(new Profile217
                        {
                            Id = profileReader.GetInt32(0),
                            Items = Json.Deserialize<List<ProfileItem217>>(profileReader.GetString(1))
                        });
                    }
                }
            }

            return profiles;
        }

        private Dictionary<int, Definition217> GetSizes()
        {
            var sizes = new Dictionary<int, Definition217>();

            using (var getDefinitionsCmd = _connection.CreateCommand())
            {
                getDefinitionsCmd.Transaction = _transaction;
                getDefinitionsCmd.CommandText = "SELECT \"Quality\", \"MinSize\", \"MaxSize\", \"PreferredSize\" FROM \"QualityDefinitions\"";

                using (var reader = getDefinitionsCmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var quality = reader.GetInt32(0);

                        double.TryParse(reader.GetValue(1).ToString(), out var minSize);
                        double.TryParse(reader.GetValue(2).ToString(), out var maxSize);
                        double.TryParse(reader.GetValue(3).ToString(), out var preferredSize);

                        sizes.Add(quality, new Definition217
                        {
                            MinSize = minSize,
                            MaxSize = maxSize,
                            PreferredSize = preferredSize
                        });
                    }
                }
            }

            return sizes;
        }
    }
}
