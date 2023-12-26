using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(003)]
    public class collections : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("MovieMetadata").AddColumn("StudioForeignId").AsInt32().Nullable()
                                        .AddColumn("StudioTitle").AsString().Nullable();

            Alter.Table("ImportLists").AddColumn("Monitor").AsInt32().Nullable();

            Alter.Table("ImportLists").AlterColumn("Monitor").AsInt32().NotNullable();

            Delete.Column("ShouldMonitor").FromTable("ImportLists");
        }
    }
}
