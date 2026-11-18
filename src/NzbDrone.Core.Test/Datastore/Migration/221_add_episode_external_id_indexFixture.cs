using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Datastore.Migration;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common.Datastore;

namespace NzbDrone.Core.Test.Datastore.Migration
{
    [TestFixture]
    public class add_episode_external_id_indexFixture : MigrationTest<add_episode_external_id_index>
    {
        [Test]
        public void should_index_episode_external_id()
        {
            var db = WithMigrationTestDb();

            var sql = PostgresDatabase.GetTestOptions().Host.IsNotNullOrWhiteSpace()
                ? "SELECT indexname FROM pg_indexes WHERE tablename = 'Episodes'"
                : "SELECT name FROM sqlite_master WHERE type = 'index' AND tbl_name = 'Episodes'";

            var indexes = db.Query(sql).Select(row => row.Values.First().ToString()).ToList();

            indexes.Should().Contain("IX_Episodes_ExternalId");
        }
    }
}
