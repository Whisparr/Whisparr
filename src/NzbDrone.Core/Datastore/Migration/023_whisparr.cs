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
            Delete.Column("MinimumAvailability").FromTable("ImportLists");

            Delete.Table("MovieTranslations");

            Create.TableForModel("Studios")
                .WithColumn("ForeignId").AsString().Unique()
                .WithColumn("QualityProfileId").AsInt32()
                .WithColumn("RootFolderPath").AsString()
                .WithColumn("SearchOnAdd").AsBoolean()
                .WithColumn("Title").AsString()
                .WithColumn("SortTitle").AsString().Nullable()
                .WithColumn("CleanTitle").AsString()
                .WithColumn("Overview").AsString().Nullable()
                .WithColumn("Website").AsString().Nullable()
                .WithColumn("Images").AsString().WithDefaultValue("[]")
                .WithColumn("Monitored").AsBoolean().WithDefaultValue(false)
                .WithColumn("LastInfoSync").AsDateTimeOffset().Nullable()
                .WithColumn("Added").AsDateTimeOffset().Nullable()
                .WithColumn("Tags").AsString();

            Create.TableForModel("Performers")
                .WithColumn("ForeignId").AsString().Unique()
                .WithColumn("QualityProfileId").AsInt32()
                .WithColumn("RootFolderPath").AsString()
                .WithColumn("SearchOnAdd").AsBoolean()
                .WithColumn("Name").AsString()
                .WithColumn("SortName").AsString().Nullable()
                .WithColumn("CleanName").AsString()
                .WithColumn("Gender").AsInt32()
                .WithColumn("Images").AsString().WithDefaultValue("[]")
                .WithColumn("Monitored").AsBoolean().WithDefaultValue(false)
                .WithColumn("LastInfoSync").AsDateTimeOffset().Nullable()
                .WithColumn("Added").AsDateTimeOffset().Nullable()
                .WithColumn("Tags").AsString();
        }
    }
}
