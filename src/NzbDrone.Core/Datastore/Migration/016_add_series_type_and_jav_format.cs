using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(016)]
    public class AddSeriesTypeAndJavFormat : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("Series").AddColumn("SeriesType").AsInt32().WithDefaultValue(0);
            Alter.Table("NamingConfig").AddColumn("JavEpisodeFormat").AsString().Nullable().WithDefaultValue("{External Id}");
        }
    }
}