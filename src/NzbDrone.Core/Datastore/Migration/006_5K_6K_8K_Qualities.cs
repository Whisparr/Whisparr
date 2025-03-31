using System.Collections.Generic;
using System.Data;
using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(6)]
    public class AddHighWebDlQualities : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Execute.WithConnection(UpdateQualityDefinitions);
        }

        private void UpdateQualityDefinitions(IDbConnection conn, IDbTransaction tran)
        {
            var updater = new ProfileUpdater125(conn, tran);
            updater.UpdateQualityToQualityDefinition();
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

        public ProfileUpdater125(IDbConnection conn, IDbTransaction tran)
        {
            _connection = conn;
            _transaction = tran;
        }

        public void Commit()
        {
        }

        public void UpdateQualityToQualityDefinition()
        {
            var definitions = new List<QualityDefinition125>();
            using (var getDefinitions = _connection.CreateCommand())
            {
                getDefinitions.Transaction = _transaction;
                getDefinitions.CommandText = @"SELECT ""Id"", ""Quality"" FROM ""QualityDefinitions""";

                using (var definitionsReader = getDefinitions.ExecuteReader())
                {
                    while (definitionsReader.Read())
                    {
                        var id = definitionsReader.GetInt32(0);
                        var quality = definitionsReader.GetInt32(1);
                        definitions.Add(new QualityDefinition125 { Id = id, Quality = quality });
                    }
                }
            }
        }
    }
}
