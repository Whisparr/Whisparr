using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(002)]
    public class postgres_update_timestamp_columns_to_with_timezone : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("VersionInfo").AlterColumn("AppliedOn").AsDateTimeOffset().Nullable();
        }

        protected override void LogDbUpgrade()
        {
            Alter.Table("VersionInfo").AlterColumn("AppliedOn").AsDateTimeOffset().Nullable();
        }
    }
}
