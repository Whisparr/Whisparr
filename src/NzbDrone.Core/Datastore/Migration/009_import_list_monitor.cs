using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(009)]
    public class import_list_monitor : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Rename.Column("ShouldMonitor").OnTable("ImportLists").To("SiteMonitorType");

            Create.Column("ShouldMonitor").OnTable("ImportLists").AsInt32().WithDefaultValue(2);
        }
    }
}
