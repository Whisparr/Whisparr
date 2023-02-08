using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(2)]
    public class remove_season_folders : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Delete.Column("SeasonFolder").FromTable("Series");
            Delete.Column("SeasonFolderFormat").FromTable("NamingConfig");
            Delete.Column("SeasonFolder").FromTable("ImportLists");
        }
    }
}
