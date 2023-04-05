using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;
using NzbDrone.Core.Languages;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(010)]
    public class movies : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            // Add movies table
            Create.TableForModel("Movies")
                  .WithColumn("TmdbId").AsInt32().Unique()
                  .WithColumn("Title").AsString()
                  .WithColumn("TitleSlug").AsString().Nullable()
                  .WithColumn("CleanTitle").AsString().Indexed()
                  .WithColumn("Status").AsInt32() // Needed?
                  .WithColumn("Overview").AsString().Nullable()
                  .WithColumn("Images").AsString()
                  .WithColumn("Path").AsString().Indexed()
                  .WithColumn("Monitored").AsBoolean()
                  .WithColumn("QualityProfileId").AsInt32()
                  .WithColumn("LastInfoSync").AsDateTimeOffset().Nullable()
                  .WithColumn("LastDiskSync").AsDateTimeOffset().Nullable()
                  .WithColumn("Runtime").AsInt32()
                  .WithColumn("Studio").AsString().Nullable()
                  .WithColumn("Year").AsInt32().Nullable()
                  .WithColumn("Ratings").AsString().Nullable()
                  .WithColumn("Genres").AsString().Nullable()
                  .WithColumn("SortTitle").AsString().Nullable()
                  .WithColumn("Tags").AsString().Nullable()
                  .WithColumn("Added").AsDateTimeOffset().Nullable()
                  .WithColumn("AddOptions").AsString().Nullable()
                  .WithColumn("MovieFileId").AsInt32().Nullable().Indexed()
                  .WithColumn("OriginalLanguage").AsInt32().WithDefaultValue((int)Language.English);

            // Add moviefiles table
            Create.TableForModel("MovieFiles")
                  .WithColumn("MovieId").AsInt32().Indexed()
                  .WithColumn("Quality").AsString()
                  .WithColumn("Size").AsInt64()
                  .WithColumn("DateAdded").AsDateTimeOffset()
                  .WithColumn("SceneName").AsString().Nullable()
                  .WithColumn("ReleaseGroup").AsString().Nullable()
                  .WithColumn("MediaInfo").AsString().Nullable()
                  .WithColumn("RelativePath").AsString().Nullable()
                  .WithColumn("Languages").AsString().NotNullable().WithDefaultValue("[]")
                  .WithColumn("OriginalFilePath").AsString().Nullable();

            // Add naming configs
            Alter.Table("NamingConfig")
                 .AddColumn("StandardMovieFormat").AsString().Nullable();

            Alter.Table("NamingConfig")
                 .AddColumn("MovieFolderFormat").AsString().Nullable();

            // Set defaults for existing configs
            Update.Table("NamingConfig").Set(new
            {
                StandardMovieFormat = "{Movie Title} ({Movie Year})",
                MovieFolderFormat = "{Movie Title} ({Movie Year}) {Quality Full}"
            }).AllRows();
        }
    }
}
