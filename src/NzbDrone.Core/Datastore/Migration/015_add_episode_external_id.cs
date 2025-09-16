using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(015)]
    public class AddEpisodeExternalId : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("Episodes").AddColumn("ExternalId").AsString().Nullable();
        }
    }
}
