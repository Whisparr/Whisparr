using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(009)]
    public class studio_aliases : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("MovieMetadata").AddColumn("Code").AsString().Nullable();
            Alter.Table("Studios").AddColumn("Aliases").AsString().Nullable();
            Alter.Table("Studios").AddColumn("AfterDate").AsDate().Nullable();
        }
    }
}
