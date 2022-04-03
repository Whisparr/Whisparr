using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Movies;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(1)]
    public class InitialSetup : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Create.TableForModel("Config")
                  .WithColumn("Key").AsString().Unique()
                  .WithColumn("Value").AsString();

            Create.TableForModel("RootFolders")
                  .WithColumn("Path").AsString().Unique();

            Create.TableForModel("Movies")
                .WithColumn("ImdbId").AsString().Nullable()
                .WithColumn("Title").AsString()
                .WithColumn("TitleSlug").AsString().Unique()
                .WithColumn("SortTitle").AsString().Nullable()
                .WithColumn("CleanTitle").AsString()
                .WithColumn("Status").AsInt32()
                .WithColumn("Overview").AsString().Nullable()
                .WithColumn("Images").AsString()
                .WithColumn("Path").AsString()
                .WithColumn("Monitored").AsBoolean()
                .WithColumn("ProfileId").AsInt32()
                .WithColumn("LastInfoSync").AsDateTime().Nullable()
                .WithColumn("LastDiskSync").AsDateTime().Nullable()
                .WithColumn("Runtime").AsInt32()
                .WithColumn("InCinemas").AsDateTime().Nullable()
                .WithColumn("Year").AsInt32().Nullable()
                .WithColumn("Added").AsDateTime().Nullable()
                .WithColumn("Ratings").AsString().Nullable()
                .WithColumn("Genres").AsString().Nullable()
                .WithColumn("Tags").AsString().Nullable()
                .WithColumn("Certification").AsString().Nullable()
                .WithColumn("AddOptions").AsString().Nullable()
                .WithColumn("MovieFileId").AsInt32().WithDefaultValue(0)
                .WithColumn("TmdbId").AsInt32().Unique()
                .WithColumn("Website").AsString().Nullable()
                .WithColumn("PhysicalRelease").AsDateTime().Nullable()
                .WithColumn("YouTubeTrailerId").AsString().Nullable()
                .WithColumn("Studio").AsString().Nullable()
                .WithColumn("SecondaryYear").AsInt32().Nullable()
                .WithColumn("Collection").AsString().Nullable()
                .WithColumn("Recommendations").AsString().WithDefaultValue("[]")
                .WithColumn("OriginalLanguage").AsInt32().WithDefaultValue((int)Language.English)
                .WithColumn("OriginalTitle").AsString().Nullable()
                .WithColumn("DigitalRelease").AsDateTime().Nullable()
                .WithColumn("MinimumAvailability").AsInt32().WithDefaultValue((int)MovieStatusType.Released);

            Create.TableForModel("History")
                .WithColumn("MovieId").AsInt32()
                .WithColumn("SourceTitle").AsString()
                .WithColumn("Date").AsDateTime()
                .WithColumn("Quality").AsString()
                .WithColumn("Data").AsString()
                .WithColumn("EventType").AsInt32().Nullable()
                .WithColumn("DownloadId").AsString().Nullable().Indexed()
                .WithColumn("Languages").AsString().NotNullable().WithDefaultValue("[]");

            Create.TableForModel("Notifications")
                .WithColumn("Name").AsString()
                .WithColumn("OnGrab").AsBoolean()
                .WithColumn("OnDownload").AsBoolean()
                .WithColumn("Settings").AsString()
                .WithColumn("Implementation").AsString()
                .WithColumn("ConfigContract").AsString().Nullable()
                .WithColumn("OnUpgrade").AsBoolean().Nullable()
                .WithColumn("Tags").AsString().Nullable()
                .WithColumn("OnRename").AsBoolean().NotNullable()
                .WithColumn("OnHealthIssue").AsBoolean().WithDefaultValue(false)
                .WithColumn("IncludeHealthWarnings").AsBoolean().WithDefaultValue(false)
                .WithColumn("OnMovieDelete").AsBoolean().WithDefaultValue(false)
                .WithColumn("OnMovieFileDelete").AsBoolean().WithDefaultValue(false)
                .WithColumn("OnMovieFileDeleteForUpgrade").AsBoolean().WithDefaultValue(false)
                .WithColumn("OnApplicationUpdate").AsBoolean().WithDefaultValue(false);

            Create.TableForModel("ScheduledTasks")
                .WithColumn("TypeName").AsString().Unique()
                .WithColumn("Interval").AsDouble()
                .WithColumn("LastExecution").AsDateTime()
                .WithColumn("LastStartTime").AsDateTime().Nullable();

            Create.TableForModel("Indexers")
                .WithColumn("Name").AsString().Unique()
                .WithColumn("Implementation").AsString()
                .WithColumn("Settings").AsString().Nullable()
                .WithColumn("ConfigContract").AsString().Nullable()
                .WithColumn("EnableRss").AsBoolean().Nullable()
                .WithColumn("EnableAutomaticSearch").AsBoolean().Nullable()
                .WithColumn("EnableInteractiveSearch").AsBoolean().Nullable()
                .WithColumn("Priority").AsInt32().NotNullable().WithDefaultValue(25)
                .WithColumn("Tags").AsString().Nullable()
                .WithColumn("DownloadClientId").AsInt32().WithDefaultValue(0);

            Create.TableForModel("Profiles")
                .WithColumn("Name").AsString().Unique()
                .WithColumn("Cutoff").AsInt32()
                .WithColumn("Items").AsString().NotNullable()
                .WithColumn("Language").AsInt32().Nullable()
                .WithColumn("UpgradeAllowed").AsBoolean().Nullable()
                .WithColumn("MinFormatScore").AsInt32().WithDefaultValue(0)
                .WithColumn("CutoffFormatScore").AsInt32().WithDefaultValue(0)
                .WithColumn("FormatItems").AsString().WithDefaultValue("[{\"format\":0, \"allowed\":true}]");

            Execute.Sql("UPDATE \"Profiles\" SET \"Language\" = 1");

            Create.TableForModel("NamingConfig")
                .WithColumn("MultiEpisodeStyle").AsInt32()
                .WithColumn("ReplaceIllegalCharacters").AsBoolean().WithDefaultValue(true)
                .WithColumn("StandardMovieFormat").AsString().Nullable()
                .WithColumn("MovieFolderFormat").AsString().Nullable()
                .WithColumn("ColonReplacementFormat").AsInt32().NotNullable().WithDefaultValue(0)
                .WithColumn("RenameMovies").AsBoolean().WithDefaultValue(false);

            Create.TableForModel("QualityDefinitions")
                .WithColumn("Quality").AsInt32().Unique()
                .WithColumn("Title").AsString().Unique()
                .WithColumn("MinSize").AsDouble().Nullable()
                .WithColumn("MaxSize").AsDouble().Nullable()
                .WithColumn("PreferredSize").AsDouble().Nullable();

            Create.TableForModel("Metadata")
                .WithColumn("Enable").AsBoolean().NotNullable()
                .WithColumn("Name").AsString().NotNullable()
                .WithColumn("Implementation").AsString().NotNullable()
                .WithColumn("Settings").AsString().NotNullable()
                .WithColumn("ConfigContract").AsString().NotNullable();

            Create.TableForModel("DownloadClients")
                .WithColumn("Enable").AsBoolean().NotNullable()
                .WithColumn("Name").AsString().NotNullable()
                .WithColumn("Implementation").AsString().NotNullable()
                .WithColumn("Settings").AsString().NotNullable()
                .WithColumn("ConfigContract").AsString().NotNullable()
                .WithColumn("Priority").AsInt32().WithDefaultValue(1)
                .WithColumn("RemoveCompletedDownloads").AsBoolean().NotNullable().WithDefaultValue(true)
                .WithColumn("RemoveFailedDownloads").AsBoolean().NotNullable().WithDefaultValue(true);

            Create.TableForModel("PendingReleases")
                .WithColumn("Title").AsString()
                .WithColumn("Added").AsDateTime()
                .WithColumn("Release").AsString()
                .WithColumn("MovieId").AsInt32().WithDefaultValue(0)
                .WithColumn("ParsedMovieInfo").AsString().Nullable()
                .WithColumn("Reason").AsInt32().WithDefaultValue(0);

            Create.TableForModel("RemotePathMappings")
                .WithColumn("Host").AsString()
                .WithColumn("RemotePath").AsString()
                .WithColumn("LocalPath").AsString();

            Create.TableForModel("Tags")
                .WithColumn("Label").AsString().Unique();

            Create.TableForModel("Restrictions")
                .WithColumn("Required").AsString().Nullable()
                .WithColumn("Preferred").AsString().Nullable()
                .WithColumn("Ignored").AsString().Nullable()
                .WithColumn("Tags").AsString().NotNullable();

            Create.TableForModel("DelayProfiles")
                .WithColumn("EnableUsenet").AsBoolean().NotNullable()
                .WithColumn("EnableTorrent").AsBoolean().NotNullable()
                .WithColumn("PreferredProtocol").AsInt32().NotNullable()
                .WithColumn("UsenetDelay").AsInt32().NotNullable()
                .WithColumn("TorrentDelay").AsInt32().NotNullable()
                .WithColumn("Order").AsInt32().NotNullable()
                .WithColumn("Tags").AsString().NotNullable()
                .WithColumn("BypassIfHighestQuality").AsBoolean().WithDefaultValue(false);

            Insert.IntoTable("DelayProfiles").Row(new
            {
                EnableUsenet = true,
                EnableTorrent = true,
                PreferredProtocol = 1,
                UsenetDelay = 0,
                TorrentDelay = 0,
                Order = int.MaxValue,
                Tags = "[]"
            });

            Create.TableForModel("Users")
                .WithColumn("Identifier").AsString().NotNullable().Unique()
                .WithColumn("Username").AsString().NotNullable().Unique()
                .WithColumn("Password").AsString().NotNullable();

            Create.TableForModel("Commands")
                .WithColumn("Name").AsString().NotNullable()
                .WithColumn("Body").AsString().NotNullable()
                .WithColumn("Priority").AsInt32().NotNullable()
                .WithColumn("Status").AsInt32().NotNullable()
                .WithColumn("QueuedAt").AsDateTime().NotNullable()
                .WithColumn("StartedAt").AsDateTime().Nullable()
                .WithColumn("EndedAt").AsDateTime().Nullable()
                .WithColumn("Duration").AsString().Nullable()
                .WithColumn("Exception").AsString().Nullable()
                .WithColumn("Trigger").AsInt32().NotNullable();

            Create.TableForModel("IndexerStatus")
                .WithColumn("ProviderId").AsInt32().NotNullable().Unique()
                .WithColumn("InitialFailure").AsDateTime().Nullable()
                .WithColumn("MostRecentFailure").AsDateTime().Nullable()
                .WithColumn("EscalationLevel").AsInt32().NotNullable()
                .WithColumn("DisabledTill").AsDateTime().Nullable()
                .WithColumn("LastRssSyncReleaseInfo").AsString().Nullable()
                .WithColumn("Cookies").AsString().Nullable()
                .WithColumn("CookiesExpirationDate").AsDateTime().Nullable();

            Create.Index().OnTable("History").OnColumn("Date");

            Create.TableForModel("MovieFiles")
                  .WithColumn("MovieId").AsInt32()
                  .WithColumn("Quality").AsString()
                  .WithColumn("Size").AsInt64()
                  .WithColumn("DateAdded").AsDateTime()
                  .WithColumn("SceneName").AsString().Nullable()
                  .WithColumn("MediaInfo").AsString().Nullable()
                  .WithColumn("ReleaseGroup").AsString().Nullable()
                  .WithColumn("RelativePath").AsString().Nullable()
                  .WithColumn("Edition").AsString().Nullable()
                  .WithColumn("Languages").AsString().NotNullable().WithDefaultValue("[]")
                  .WithColumn("IndexerFlags").AsInt32().WithDefaultValue(0)
                  .WithColumn("OriginalFilePath").AsString().Nullable();

            Create.TableForModel("ImportLists")
                    .WithColumn("Enabled").AsBoolean()
                    .WithColumn("Name").AsString().Unique()
                    .WithColumn("Implementation").AsString()
                    .WithColumn("ConfigContract").AsString().Nullable()
                    .WithColumn("Settings").AsString().Nullable()
                    .WithColumn("EnableAuto").AsBoolean()
                    .WithColumn("RootFolderPath").AsString()
                    .WithColumn("ShouldMonitor").AsBoolean()
                    .WithColumn("ProfileId").AsInt32()
                    .WithColumn("MinimumAvailability").AsInt32().WithDefaultValue((int)MovieStatusType.Released)
                    .WithColumn("Tags").AsString().Nullable()
                    .WithColumn("SearchOnAdd").AsBoolean().WithDefaultValue(false);

            Create.TableForModel("ImportExclusions")
                    .WithColumn("TmdbId").AsInt64().NotNullable().Unique().PrimaryKey()
                    .WithColumn("MovieTitle").AsString().Nullable()
                    .WithColumn("MovieYear").AsInt64().Nullable().WithDefaultValue(0);

            Create.TableForModel("AlternativeTitles")
                      .WithColumn("MovieId").AsInt64().NotNullable()
                      .WithColumn("Title").AsString().NotNullable()
                      .WithColumn("CleanTitle").AsString().Unique()
                      .WithColumn("SourceType").AsInt64().WithDefaultValue(0)
                      .WithColumn("SourceId").AsInt64().WithDefaultValue(0)
                      .WithColumn("Votes").AsInt64().WithDefaultValue(0)
                      .WithColumn("VoteCount").AsInt64().WithDefaultValue(0)
                      .WithColumn("Language").AsInt64().WithDefaultValue(0);

            Create.TableForModel("ExtraFiles")
                .WithColumn("MovieId").AsInt32().NotNullable()
                .WithColumn("MovieFileId").AsInt32().NotNullable()
                .WithColumn("RelativePath").AsString().NotNullable()
                .WithColumn("Extension").AsString().NotNullable()
                .WithColumn("Added").AsDateTime().NotNullable()
                .WithColumn("LastUpdated").AsDateTime().NotNullable();

            Create.TableForModel("SubtitleFiles")
                .WithColumn("MovieId").AsInt32().NotNullable()
                .WithColumn("MovieFileId").AsInt32().NotNullable()
                .WithColumn("RelativePath").AsString().NotNullable()
                .WithColumn("Extension").AsString().NotNullable()
                .WithColumn("Added").AsDateTime().NotNullable()
                .WithColumn("LastUpdated").AsDateTime().NotNullable()
                .WithColumn("Language").AsInt32().NotNullable();

            Create.TableForModel("MetadataFiles")
                .WithColumn("MovieId").AsInt32().NotNullable()
                .WithColumn("Consumer").AsString().NotNullable()
                .WithColumn("Type").AsInt32().NotNullable()
                .WithColumn("RelativePath").AsString().NotNullable()
                .WithColumn("LastUpdated").AsDateTime().NotNullable()
                .WithColumn("MovieFileId").AsInt32().Nullable()
                .WithColumn("Hash").AsString().Nullable()
                .WithColumn("Added").AsDateTime().Nullable()
                .WithColumn("Extension").AsString().NotNullable();

            Create.TableForModel("CustomFilters")
                  .WithColumn("Type").AsString().NotNullable()
                  .WithColumn("Label").AsString().NotNullable()
                  .WithColumn("Filters").AsString().NotNullable();

            Create.TableForModel("DownloadClientStatus")
                    .WithColumn("ProviderId").AsInt32().NotNullable().Unique()
                    .WithColumn("InitialFailure").AsDateTime().Nullable()
                    .WithColumn("MostRecentFailure").AsDateTime().Nullable()
                    .WithColumn("EscalationLevel").AsInt32().NotNullable()
                    .WithColumn("DisabledTill").AsDateTime().Nullable();

            Create.Index("IX_MovieFiles_MovieId").OnTable("MovieFiles").OnColumn("MovieId");
            Create.Index("IX_AlternativeTitles_MovieId").OnTable("AlternativeTitles").OnColumn("MovieId");

            // Speed up release processing (these are present in Sonarr)
            Create.Index("IX_Movies_CleanTitle").OnTable("Movies").OnColumn("CleanTitle");
            Create.Index("IX_Movies_ImdbId").OnTable("Movies").OnColumn("ImdbId");

            Create.TableForModel("Credits").WithColumn("MovieId").AsInt32()
                      .WithColumn("CreditTmdbId").AsString().Unique()
                      .WithColumn("PersonTmdbId").AsInt32()
                      .WithColumn("Name").AsString()
                      .WithColumn("Images").AsString()
                      .WithColumn("Character").AsString().Nullable()
                      .WithColumn("Order").AsInt32()
                      .WithColumn("Job").AsString().Nullable()
                      .WithColumn("Department").AsString().Nullable()
                      .WithColumn("Type").AsInt32();

            Create.Index().OnTable("Credits").OnColumn("MovieId");

            Create.TableForModel("DownloadHistory")
                  .WithColumn("EventType").AsInt32().NotNullable()
                  .WithColumn("MovieId").AsInt32().NotNullable()
                  .WithColumn("DownloadId").AsString().NotNullable()
                  .WithColumn("SourceTitle").AsString().NotNullable()
                  .WithColumn("Date").AsDateTime().NotNullable()
                  .WithColumn("Protocol").AsInt32().Nullable()
                  .WithColumn("IndexerId").AsInt32().Nullable()
                  .WithColumn("DownloadClientId").AsInt32().Nullable()
                  .WithColumn("Release").AsString().Nullable()
                  .WithColumn("Data").AsString().Nullable();

            Create.Index().OnTable("DownloadHistory").OnColumn("EventType");
            Create.Index().OnTable("DownloadHistory").OnColumn("MovieId");
            Create.Index().OnTable("DownloadHistory").OnColumn("DownloadId");

            Create.TableForModel("ImportListStatus")
                .WithColumn("ProviderId").AsInt32().NotNullable().Unique()
                .WithColumn("InitialFailure").AsDateTime().Nullable()
                .WithColumn("MostRecentFailure").AsDateTime().Nullable()
                .WithColumn("EscalationLevel").AsInt32().NotNullable()
                .WithColumn("DisabledTill").AsDateTime().Nullable()
                .WithColumn("LastSyncListInfo").AsString().Nullable();

            //Manual SQL, Fluent Migrator doesn't support multi-column unique contraint on table creation, SQLite doesn't support adding it after creation
            IfDatabase("sqlite").Execute.Sql("CREATE TABLE MovieTranslations(" +
                "Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, " +
                "MovieId INTEGER NOT NULL, " +
                "Title TEXT, " +
                "CleanTitle TEXT, " +
                "Overview TEXT, " +
                "Language INTEGER NOT NULL, " +
                "Unique(\"MovieId\", \"Language\"));");

            IfDatabase("postgres").Execute.Sql("CREATE TABLE \"MovieTranslations\"(" +
                "\"Id\" SERIAL PRIMARY KEY , " +
                "\"MovieId\" INTEGER NOT NULL, " +
                "\"Title\" TEXT, " +
                "\"CleanTitle\" TEXT, " +
                "\"Overview\" TEXT, " +
                "\"Language\" INTEGER NOT NULL, " +
                "Unique(\"MovieId\", \"Language\"));");

            // Prevent failure if two movies have same alt titles
            Execute.Sql("DROP INDEX IF EXISTS \"IX_AlternativeTitles_CleanTitle\"");

            Create.Index("IX_MovieTranslations_Language").OnTable("MovieTranslations").OnColumn("Language");
            Create.Index("IX_MovieTranslations_MovieId").OnTable("MovieTranslations").OnColumn("MovieId");

            Create.TableForModel("ImportListMovies")
                .WithColumn("ImdbId").AsString().Nullable()
                .WithColumn("TmdbId").AsInt32()
                .WithColumn("ListId").AsInt32()
                .WithColumn("Title").AsString()
                .WithColumn("SortTitle").AsString().Nullable()
                .WithColumn("Status").AsInt32()
                .WithColumn("Overview").AsString().Nullable()
                .WithColumn("Images").AsString()
                .WithColumn("LastInfoSync").AsDateTime().Nullable()
                .WithColumn("Runtime").AsInt32()
                .WithColumn("InCinemas").AsDateTime().Nullable()
                .WithColumn("Year").AsInt32().Nullable()
                .WithColumn("Ratings").AsString().Nullable()
                .WithColumn("Genres").AsString().Nullable()
                .WithColumn("Certification").AsString().Nullable()
                .WithColumn("Collection").AsString().Nullable()
                .WithColumn("Website").AsString().Nullable()
                .WithColumn("OriginalTitle").AsString().Nullable()
                .WithColumn("PhysicalRelease").AsDateTime().Nullable()
                .WithColumn("Translations").AsString()
                .WithColumn("Studio").AsString().Nullable()
                .WithColumn("YouTubeTrailerId").AsString().Nullable()
                .WithColumn("DigitalRelease").AsDateTime().Nullable();

            Create.Index().OnTable("AlternativeTitles").OnColumn("CleanTitle");
            Create.Index().OnTable("MovieTranslations").OnColumn("CleanTitle");

            Create.TableForModel("Blocklist")
                .WithColumn("SourceTitle").AsString()
                .WithColumn("Quality").AsString()
                .WithColumn("Date").AsDateTime()
                .WithColumn("PublishedDate").AsDateTime().Nullable()
                .WithColumn("Size").AsInt64().Nullable()
                .WithColumn("Protocol").AsInt32().Nullable()
                .WithColumn("Indexer").AsString().Nullable()
                .WithColumn("Message").AsString().Nullable()
                .WithColumn("TorrentInfoHash").AsString().Nullable()
                .WithColumn("MovieId").AsInt32().WithDefaultValue(0)
                .WithColumn("Languages").AsString().NotNullable().WithDefaultValue("[]")
                .WithColumn("IndexerFlags").AsInt32().WithDefaultValue(0);

            Create.TableForModel("CustomFormats")
                .WithColumn("Name").AsString().Unique()
                .WithColumn("Specifications").AsString().WithDefaultValue("[]")
                .WithColumn("IncludeCustomFormatWhenRenaming").AsBoolean().WithDefaultValue(false);
        }

        protected override void LogDbUpgrade()
        {
            Create.TableForModel("Logs")
                  .WithColumn("Message").AsString()
                  .WithColumn("Time").AsDateTime().Indexed()
                  .WithColumn("Logger").AsString()
                  .WithColumn("Exception").AsString().Nullable()
                  .WithColumn("ExceptionType").AsString().Nullable()
                  .WithColumn("Level").AsString();

            Create.TableForModel("UpdateHistory")
                  .WithColumn("Date").AsDateTime().NotNullable().Indexed()
                  .WithColumn("Version").AsString().NotNullable()
                  .WithColumn("EventType").AsInt32().NotNullable();
        }
    }
}
