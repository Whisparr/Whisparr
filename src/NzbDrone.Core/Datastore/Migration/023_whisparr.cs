using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(023)]
    public class whisparr : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Delete.Column("SecondaryYear").FromTable("MovieMetadata");
            Delete.Column("Certification").FromTable("MovieMetadata");
            Delete.Column("YouTubeTrailerId").FromTable("MovieMetadata");
            Delete.Column("Popularity").FromTable("MovieMetadata");
            Delete.Column("OriginalTitle").FromTable("MovieMetadata");
            Delete.Column("CleanOriginalTitle").FromTable("MovieMetadata");

            Delete.Column("MinimumAvailability").FromTable("Movies");
            Delete.Column("MinimumAvailability").FromTable("Collections");
            Delete.Column("MinimumAvailability").FromTable("ImportLists");

            Delete.Table("MovieTranslations");
        }
    }
}
