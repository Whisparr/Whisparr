using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

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
                .WithColumn("Path").AsString()
                .WithColumn("Monitored").AsBoolean()
                .WithColumn("QualityProfileId").AsInt32()
                .WithColumn("Added").AsDateTimeOffset().Nullable()
                .WithColumn("Tags").AsString().Nullable()
                .WithColumn("AddOptions").AsString().Nullable()
                .WithColumn("MovieFileId").AsInt32().WithDefaultValue(0)
                .WithColumn("MovieMetadataId").AsInt32().Unique();

            Create.TableForModel("History")
                .WithColumn("MovieId").AsInt32()
                .WithColumn("SourceTitle").AsString()
                .WithColumn("Date").AsDateTimeOffset()
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
                .WithColumn("LastExecution").AsDateTimeOffset()
                .WithColumn("LastStartTime").AsDateTimeOffset().Nullable();

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

            Create.TableForModel("QualityProfiles")
                .WithColumn("Name").AsString().Unique()
                .WithColumn("Cutoff").AsInt32()
                .WithColumn("Items").AsString().NotNullable()
                .WithColumn("Language").AsInt32().Nullable()
                .WithColumn("UpgradeAllowed").AsBoolean().Nullable()
                .WithColumn("MinFormatScore").AsInt32().WithDefaultValue(0)
                .WithColumn("CutoffFormatScore").AsInt32().WithDefaultValue(0)
                .WithColumn("FormatItems").AsString().WithDefaultValue("[{\"format\":0, \"allowed\":true}]");

            Execute.Sql("UPDATE \"QualityProfiles\" SET \"Language\" = 1");

            Create.TableForModel("NamingConfig")
                .WithColumn("MultiEpisodeStyle").AsInt32()
                .WithColumn("ReplaceIllegalCharacters").AsBoolean().WithDefaultValue(true)
                .WithColumn("StandardMovieFormat").AsString().Nullable()
                .WithColumn("MovieFolderFormat").AsString().Nullable()
                .WithColumn("StandardSceneFormat").AsString().Nullable()
                .WithColumn("SceneFolderFormat").AsString().Nullable()
                .WithColumn("ColonReplacementFormat").AsInt32().NotNullable().WithDefaultValue(0)
                .WithColumn("RenameMovies").AsBoolean().WithDefaultValue(false)
                .WithColumn("RenameScenes").AsBoolean().WithDefaultValue(false);

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
                .WithColumn("Added").AsDateTimeOffset()
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

            Create.TableForModel("ReleaseProfiles")
                .WithColumn("Required").AsString().Nullable()
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
                .WithColumn("QueuedAt").AsDateTimeOffset().NotNullable()
                .WithColumn("StartedAt").AsDateTimeOffset().Nullable()
                .WithColumn("EndedAt").AsDateTimeOffset().Nullable()
                .WithColumn("Duration").AsString().Nullable()
                .WithColumn("Exception").AsString().Nullable()
                .WithColumn("Trigger").AsInt32().NotNullable();

            Create.TableForModel("IndexerStatus")
                .WithColumn("ProviderId").AsInt32().NotNullable().Unique()
                .WithColumn("InitialFailure").AsDateTimeOffset().Nullable()
                .WithColumn("MostRecentFailure").AsDateTimeOffset().Nullable()
                .WithColumn("EscalationLevel").AsInt32().NotNullable()
                .WithColumn("DisabledTill").AsDateTimeOffset().Nullable()
                .WithColumn("LastRssSyncReleaseInfo").AsString().Nullable()
                .WithColumn("Cookies").AsString().Nullable()
                .WithColumn("CookiesExpirationDate").AsDateTimeOffset().Nullable();

            Create.Index().OnTable("History").OnColumn("Date");

            Create.TableForModel("MovieFiles")
                  .WithColumn("MovieId").AsInt32()
                  .WithColumn("Quality").AsString()
                  .WithColumn("Size").AsInt64()
                  .WithColumn("DateAdded").AsDateTimeOffset()
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
                    .WithColumn("QualityProfileId").AsInt32()
                    .WithColumn("Tags").AsString().Nullable()
                    .WithColumn("SearchOnAdd").AsBoolean().WithDefaultValue(false)
                    .WithColumn("Monitor").AsInt32().NotNullable();

            Create.TableForModel("ImportExclusions")
                    .WithColumn("ForeignId").AsString().NotNullable().Unique().PrimaryKey()
                    .WithColumn("MovieTitle").AsString().Nullable()
                    .WithColumn("MovieYear").AsInt64().Nullable().WithDefaultValue(0);

            Create.TableForModel("AlternativeTitles")
                      .WithColumn("Title").AsString().NotNullable()
                      .WithColumn("CleanTitle").AsString().Unique()
                      .WithColumn("SourceType").AsInt64().WithDefaultValue(0)
                      .WithColumn("MovieMetadataId").AsInt32().WithDefaultValue(0).Indexed();

            Create.TableForModel("ExtraFiles")
                .WithColumn("MovieId").AsInt32().NotNullable()
                .WithColumn("MovieFileId").AsInt32().NotNullable()
                .WithColumn("RelativePath").AsString().NotNullable()
                .WithColumn("Extension").AsString().NotNullable()
                .WithColumn("Added").AsDateTimeOffset().NotNullable()
                .WithColumn("LastUpdated").AsDateTimeOffset().NotNullable();

            Create.TableForModel("SubtitleFiles")
                .WithColumn("MovieId").AsInt32().NotNullable()
                .WithColumn("MovieFileId").AsInt32().NotNullable()
                .WithColumn("RelativePath").AsString().NotNullable()
                .WithColumn("Extension").AsString().NotNullable()
                .WithColumn("Added").AsDateTimeOffset().NotNullable()
                .WithColumn("LastUpdated").AsDateTimeOffset().NotNullable()
                .WithColumn("Language").AsInt32().NotNullable();

            Create.TableForModel("MetadataFiles")
                .WithColumn("MovieId").AsInt32().NotNullable()
                .WithColumn("Consumer").AsString().NotNullable()
                .WithColumn("Type").AsInt32().NotNullable()
                .WithColumn("RelativePath").AsString().NotNullable()
                .WithColumn("LastUpdated").AsDateTimeOffset().NotNullable()
                .WithColumn("MovieFileId").AsInt32().Nullable()
                .WithColumn("Hash").AsString().Nullable()
                .WithColumn("Added").AsDateTimeOffset().Nullable()
                .WithColumn("Extension").AsString().NotNullable();

            Create.TableForModel("CustomFilters")
                  .WithColumn("Type").AsString().NotNullable()
                  .WithColumn("Label").AsString().NotNullable()
                  .WithColumn("Filters").AsString().NotNullable();

            Create.TableForModel("DownloadClientStatus")
                    .WithColumn("ProviderId").AsInt32().NotNullable().Unique()
                    .WithColumn("InitialFailure").AsDateTimeOffset().Nullable()
                    .WithColumn("MostRecentFailure").AsDateTimeOffset().Nullable()
                    .WithColumn("EscalationLevel").AsInt32().NotNullable()
                    .WithColumn("DisabledTill").AsDateTimeOffset().Nullable();

            Create.Index("IX_MovieFiles_MovieId").OnTable("MovieFiles").OnColumn("MovieId");

            Create.TableForModel("DownloadHistory")
                  .WithColumn("EventType").AsInt32().NotNullable()
                  .WithColumn("MovieId").AsInt32().NotNullable()
                  .WithColumn("DownloadId").AsString().NotNullable()
                  .WithColumn("SourceTitle").AsString().NotNullable()
                  .WithColumn("Date").AsDateTimeOffset().NotNullable()
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
                .WithColumn("InitialFailure").AsDateTimeOffset().Nullable()
                .WithColumn("MostRecentFailure").AsDateTimeOffset().Nullable()
                .WithColumn("EscalationLevel").AsInt32().NotNullable()
                .WithColumn("DisabledTill").AsDateTimeOffset().Nullable();

            // Prevent failure if two movies have same alt titles
            Execute.Sql("DROP INDEX IF EXISTS \"IX_AlternativeTitles_CleanTitle\"");

            Create.TableForModel("ImportListMovies")
                .WithColumn("ListId").AsInt32()
                .WithColumn("MovieMetadataId").AsInt32().WithDefaultValue(0).Indexed();

            Create.Index().OnTable("AlternativeTitles").OnColumn("CleanTitle");

            Create.TableForModel("Blocklist")
                .WithColumn("SourceTitle").AsString()
                .WithColumn("Quality").AsString()
                .WithColumn("Date").AsDateTimeOffset()
                .WithColumn("PublishedDate").AsDateTimeOffset().Nullable()
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

            Create.TableForModel("MovieMetadata")
                .WithColumn("ForeignId").AsString().Unique()
                .WithColumn("MetadataSource").AsInt32()
                .WithColumn("ImdbId").AsString().Nullable().Indexed()
                .WithColumn("TmdbId").AsInt32().Nullable().Indexed()
                .WithColumn("StashId").AsString().Nullable().Indexed()
                .WithColumn("Images").AsString()
                .WithColumn("Genres").AsString().Nullable()
                .WithColumn("Title").AsString()
                .WithColumn("SortTitle").AsString().Nullable()
                .WithColumn("CleanTitle").AsString().Nullable().Indexed()
                .WithColumn("OriginalLanguage").AsInt32()
                .WithColumn("Status").AsInt32()
                .WithColumn("LastInfoSync").AsDateTimeOffset().Nullable()
                .WithColumn("Runtime").AsInt32()
                .WithColumn("ReleaseDate").AsString().Nullable()
                .WithColumn("ReleaseDateUtc").AsDateTimeOffset().Nullable()
                .WithColumn("Year").AsInt32().Nullable()
                .WithColumn("Ratings").AsString().Nullable()
                .WithColumn("Recommendations").AsString()
                .WithColumn("Overview").AsString().Nullable()
                .WithColumn("Website").AsString().Nullable()
                .WithColumn("Credits").AsString()
                .WithColumn("ItemType").AsInt32().NotNullable().Indexed()
                .WithColumn("StudioForeignId").AsString().Nullable().Indexed()
                .WithColumn("StudioTitle").AsString().Nullable();

            Alter.Table("Notifications").AddColumn("OnMovieAdded").AsBoolean().WithDefaultValue(false);

            Alter.Table("SubtitleFiles").AddColumn("LanguageTags").AsString().Nullable();

            Alter.Table("Users")
                .AddColumn("Salt").AsString().Nullable()
                .AddColumn("Iterations").AsInt32().Nullable();

            Alter.Table("PendingReleases").AddColumn("AdditionalInfo").AsString().Nullable();

            Alter.Table("Commands").AddColumn("Result").AsInt32().WithDefaultValue(1);

            Alter.Table("Notifications").AddColumn("OnHealthRestored").AsBoolean().WithDefaultValue(false);

            Alter.Table("Notifications").AddColumn("OnManualInteractionRequired").AsBoolean().WithDefaultValue(false);

            Alter.Table("ImportListStatus").AddColumn("LastInfoSync").AsDateTimeOffset().Nullable();

            Alter.Table("DownloadClients").AddColumn("Tags").AsString().Nullable();

            Create.TableForModel("AutoTagging")
                .WithColumn("Name").AsString().Unique()
                .WithColumn("Specifications").AsString().WithDefaultValue("[]")
                .WithColumn("RemoveTagsAutomatically").AsBoolean().WithDefaultValue(false)
                .WithColumn("Tags").AsString().WithDefaultValue("[]");

            Alter.Table("DelayProfiles").AddColumn("BypassIfAboveCustomFormatScore").AsBoolean().WithDefaultValue(false);
            Alter.Table("DelayProfiles").AddColumn("MinimumCustomFormatScore").AsInt32().Nullable();

            Alter.Table("ReleaseProfiles").AddColumn("Name").AsString().Nullable().WithDefaultValue(null);
            Alter.Table("ReleaseProfiles").AddColumn("Enabled").AsBoolean().WithDefaultValue(true);
            Alter.Table("ReleaseProfiles").AddColumn("IndexerId").AsInt32().WithDefaultValue(0);

            Create.TableForModel("NotificationStatus")
                  .WithColumn("ProviderId").AsInt32().NotNullable().Unique()
                  .WithColumn("InitialFailure").AsDateTimeOffset().Nullable()
                  .WithColumn("MostRecentFailure").AsDateTimeOffset().Nullable()
                  .WithColumn("EscalationLevel").AsInt32().NotNullable()
                  .WithColumn("DisabledTill").AsDateTimeOffset().Nullable();

            Create.TableForModel("Studios")
                .WithColumn("ForeignId").AsString().Unique()
                .WithColumn("QualityProfileId").AsInt32()
                .WithColumn("RootFolderPath").AsString()
                .WithColumn("SearchOnAdd").AsBoolean()
                .WithColumn("Title").AsString()
                .WithColumn("SortTitle").AsString().Nullable()
                .WithColumn("CleanTitle").AsString()
                .WithColumn("Overview").AsString().Nullable()
                .WithColumn("Network").AsString().Nullable()
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
                .WithColumn("Ethnicity").AsInt32().Nullable()
                .WithColumn("HairColor").AsInt32().Nullable()
                .WithColumn("Age").AsInt32().Nullable()
                .WithColumn("CareerStart").AsInt32().Nullable()
                .WithColumn("CareerEnd").AsInt32().Nullable()
                .WithColumn("Status").AsInt32()
                .WithColumn("Images").AsString().WithDefaultValue("[]")
                .WithColumn("Monitored").AsBoolean().WithDefaultValue(false)
                .WithColumn("LastInfoSync").AsDateTimeOffset().Nullable()
                .WithColumn("Added").AsDateTimeOffset().Nullable()
                .WithColumn("Tags").AsString();
        }

        protected override void LogDbUpgrade()
        {
            Create.TableForModel("Logs")
                  .WithColumn("Message").AsString()
                  .WithColumn("Time").AsDateTimeOffset().Indexed()
                  .WithColumn("Logger").AsString()
                  .WithColumn("Exception").AsString().Nullable()
                  .WithColumn("ExceptionType").AsString().Nullable()
                  .WithColumn("Level").AsString();

            Create.TableForModel("UpdateHistory")
                  .WithColumn("Date").AsDateTimeOffset().NotNullable().Indexed()
                  .WithColumn("Version").AsString().NotNullable()
                  .WithColumn("EventType").AsInt32().NotNullable();
        }
    }
}
