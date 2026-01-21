using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(17)]
    public class import_exclusion_type : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            // SQLite handles the conversion automatically
            IfDatabase("sqlite").Alter.Table("ImportListExclusions").AlterColumn("TvdbId").AsInt32();

            // PostgreSQL requires explicit USING clause for type conversion
            IfDatabase("postgres").Execute.Sql("ALTER TABLE \"ImportListExclusions\" ALTER COLUMN \"TvdbId\" TYPE integer USING \"TvdbId\"::integer");
        }
    }
}
