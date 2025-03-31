using System.Collections.Generic;
using System.Data;
using System.Linq;
using FluentMigrator;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(6)]
    public class AddHighWebDlQualities : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Execute.WithConnection(PerformHighWebDlQualityMigration);
        }

        private void PerformHighWebDlQualityMigration(IDbConnection conn, IDbTransaction tran)
        {
            var updater = new ProfileUpdater125(conn, tran);

            updater.SplitQualityAppend(18, 33);
            updater.SplitQualityAppend(33, 34);
            updater.SplitQualityAppend(34, 35);

            updater.Commit();
        }
    }

    public class Profile125
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Cutoff { get; set; }
        public List<ProfileItem125> Items { get; set; }
        public int Language { get; set; }
        public List<string> PreferredTags { get; set; }
    }

    public class ProfileItem125
    {
        public int? QualityDefinition { get; set; }
        public int? Quality { get; set; }
        public bool Allowed { get; set; }
    }

    public class QualityDefinition125
    {
        public int Id { get; set; }
        public int Quality { get; set; }
    }

    public class ProfileUpdater125
    {
        private readonly IDbConnection _connection;
        private readonly IDbTransaction _transaction;

        private List<Profile125> _profiles;
        private HashSet<Profile125> _changedProfiles = new HashSet<Profile125>();

        public ProfileUpdater125(IDbConnection conn, IDbTransaction tran)
        {
            _connection = conn;
            _transaction = tran;

            _profiles = GetProfiles();
        }

        public void Commit()
        {
            foreach (var profile in _changedProfiles)
            {
                using (var updateProfileCmd = _connection.CreateCommand())
                {
                    updateProfileCmd.Transaction = _transaction;
                    updateProfileCmd.CommandText = "UPDATE \"Profiles\" SET \"Name\" = ?, \"Cutoff\" = ?, \"Items\" = ?, \"Language\" = ? WHERE \"Id\" = ?";
                    updateProfileCmd.AddParameter(profile.Name);
                    updateProfileCmd.AddParameter(profile.Cutoff);
                    updateProfileCmd.AddParameter(profile.Items.ToJson());
                    updateProfileCmd.AddParameter(profile.Language);
                    updateProfileCmd.AddParameter(profile.Id);

                    updateProfileCmd.ExecuteNonQuery();
                }
            }

            _changedProfiles.Clear();
        }

        public void SplitQualityAppend(int find, int quality)
        {
            foreach (var profile in _profiles)
            {
                if (profile.Items.Any(v => v.Quality == quality))
                {
                    continue;
                }

                var findIndex = profile.Items.FindIndex(v => v.Quality == find);

                profile.Items.Insert(findIndex + 1, new ProfileItem125
                {
                    Quality = quality,
                    Allowed = false
                });

                _changedProfiles.Add(profile);
            }
        }

        private List<Profile125> GetProfiles()
        {
            var profiles = new List<Profile125>();

            using (var getProfilesCmd = _connection.CreateCommand())
            {
                getProfilesCmd.Transaction = _transaction;
                getProfilesCmd.CommandText = @"SELECT ""Id"", ""Name"", ""Cutoff"", ""Items"", ""Language"" FROM ""QualityProfiles""";

                using (var profileReader = getProfilesCmd.ExecuteReader())
                {
                    while (profileReader.Read())
                    {
                        profiles.Add(new Profile125
                        {
                            Id = profileReader.GetInt32(0),
                            Name = profileReader.GetString(1),
                            Cutoff = profileReader.GetInt32(2),
                            Items = Json.Deserialize<List<ProfileItem125>>(profileReader.GetString(3)),
                            Language = profileReader.IsDBNull(4) ? 0 : profileReader.GetInt32(4)
                        });
                    }
                }
            }

            return profiles;
        }
    }
}
