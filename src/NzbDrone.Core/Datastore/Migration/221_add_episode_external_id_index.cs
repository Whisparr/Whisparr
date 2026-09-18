using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(221)]
    public class add_episode_external_id_index : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            // Matching a release by its external ID (e.g. a JAV code) otherwise scans every episode
            Create.Index("IX_Episodes_ExternalId").OnTable("Episodes").OnColumn("ExternalId").Ascending();
        }
    }
}
