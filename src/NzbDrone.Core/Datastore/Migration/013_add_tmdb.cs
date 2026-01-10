using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(013)]
    public class add_tmdb : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("Performers")
                .AddColumn("TmdbId").AsInt32().WithDefaultValue(0);

            Alter.Table("Studios")
                .AddColumn("TmdbId").AsInt32().WithDefaultValue(0);
        }
    }
}
