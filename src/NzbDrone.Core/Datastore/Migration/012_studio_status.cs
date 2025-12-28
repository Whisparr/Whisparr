using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(012)]
    public class tudio_status : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("Studios")
                .AddColumn("Status").AsInt32().WithDefaultValue(0);
        }
    }
}
