using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(005)]
    public class import_exclusion_type : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("ImportExclusions").AddColumn("Type").AsString().Nullable();
            Alter.Table("Studios").AddColumn("SearchTitle").AsString().Nullable();
            Alter.Table("Studios").AddColumn("CleanSearchTitle").AsString().Nullable();
        }
    }
}
