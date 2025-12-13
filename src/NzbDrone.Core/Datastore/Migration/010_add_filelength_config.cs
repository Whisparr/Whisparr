using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(010)]
    public class add_filelength_config : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("NamingConfig")
                .AddColumn("MaxFolderPathLength").AsInt32().Nullable()
                .AddColumn("MaxFilePathLength").AsInt32().Nullable();
        }
    }
}
