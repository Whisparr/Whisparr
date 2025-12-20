using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(011)]
    public class add_tpdb : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("Performers")
                .AddColumn("TpdbId").AsString().Nullable();

            Alter.Table("Studios")
                .AddColumn("TpdbId").AsString().Nullable();

            Alter.Table("MovieMetadata")
                .AddColumn("TpdbId").AsString().Nullable();
        }
    }
}
