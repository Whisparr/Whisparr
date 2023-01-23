using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;
using NzbDrone.Core.Languages;

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

            Create.TableForModel("Series")
                  .WithColumn("TvdbId").AsInt32().Unique()
                  .WithColumn("Title").AsString()
                  .WithColumn("TitleSlug").AsString().Nullable()
                  .WithColumn("CleanTitle").AsString().Indexed()
                  .WithColumn("Status").AsInt32()
                  .WithColumn("Overview").AsString().Nullable()
                  .WithColumn("Images").AsString()
                  .WithColumn("Path").AsString().Indexed()
                  .WithColumn("Monitored").AsBoolean()
                  .WithColumn("QualityProfileId").AsInt32()
                  .WithColumn("SeasonFolder").AsBoolean()
                  .WithColumn("LastInfoSync").AsDateTimeOffset().Nullable()
                  .WithColumn("LastDiskSync").AsDateTimeOffset().Nullable()
                  .WithColumn("Runtime").AsInt32()
                  .WithColumn("Network").AsString().Nullable()
                  .WithColumn("UseSceneNumbering").AsBoolean()
                  .WithColumn("FirstAired").AsDateTimeOffset().Nullable()
                  .WithColumn("NextAiring").AsDateTimeOffset().Nullable()
                  .WithColumn("Year").AsInt32().Nullable()
                  .WithColumn("Seasons").AsString().Nullable()
                  .WithColumn("Ratings").AsString().Nullable()
                  .WithColumn("Genres").AsString().Nullable()
                  .WithColumn("Certification").AsString().Nullable()
                  .WithColumn("SortTitle").AsString().Nullable()
                  .WithColumn("Tags").AsString().Nullable()
                  .WithColumn("Added").AsDateTimeOffset().Nullable()
                  .WithColumn("AddOptions").AsString().Nullable()
                  .WithColumn("OriginalLanguage").AsInt32().WithDefaultValue((int)Language.English);

            Create.TableForModel("Episodes")
                  .WithColumn("Monitored").AsBoolean().Nullable()
                  .WithColumn("SeriesId").AsInt32().Indexed()
                  .WithColumn("SeasonNumber").AsInt32()
                  .WithColumn("EpisodeNumber").AsInt32()
                  .WithColumn("Title").AsString().Nullable()
                  .WithColumn("Overview").AsString().Nullable()
                  .WithColumn("Runtime").AsInt32()
                  .WithColumn("EpisodeFileId").AsInt32().Nullable().Indexed()
                  .WithColumn("AirDate").AsString().Nullable()
                  .WithColumn("AbsoluteEpisodeNumber").AsInt32().Nullable()
                  .WithColumn("SceneAbsoluteEpisodeNumber").AsInt32().Nullable()
                  .WithColumn("SceneSeasonNumber").AsInt32().Nullable()
                  .WithColumn("SceneEpisodeNumber").AsInt32().Nullable()
                  .WithColumn("AirDateUtc").AsDateTimeOffset().Nullable()
                  .WithColumn("Ratings").AsString().Nullable()
                  .WithColumn("Images").AsString().Nullable()
                  .WithColumn("Actors").AsString().Nullable()
                  .WithColumn("UnverifiedSceneNumbering").AsBoolean().WithDefaultValue(false)
                  .WithColumn("LastSearchTime").AsDateTimeOffset().Nullable()
                  .WithColumn("AiredAfterSeasonNumber").AsInt32().Nullable()
                  .WithColumn("AiredBeforeSeasonNumber").AsInt32().Nullable()
                  .WithColumn("AiredBeforeEpisodeNumber").AsInt32().Nullable()
                  .WithColumn("TvdbId").AsInt32().Nullable();

            Create.Index().OnTable("Episodes").OnColumn("SeriesId").Ascending()
                                              .OnColumn("SeasonNumber").Ascending()
                                              .OnColumn("EpisodeNumber").Ascending();

            Create.Index().OnTable("Episodes").OnColumn("SeriesId").Ascending()
                                              .OnColumn("AirDate").Ascending();

            Create.TableForModel("EpisodeFiles")
                  .WithColumn("SeriesId").AsInt32().Indexed()
                  .WithColumn("Quality").AsString()
                  .WithColumn("Size").AsInt64()
                  .WithColumn("DateAdded").AsDateTimeOffset()
                  .WithColumn("SeasonNumber").AsInt32()
                  .WithColumn("SceneName").AsString().Nullable()
                  .WithColumn("ReleaseGroup").AsString().Nullable()
                  .WithColumn("MediaInfo").AsString().Nullable()
                  .WithColumn("RelativePath").AsString().Nullable()
                  .WithColumn("Languages").AsString().NotNullable().WithDefaultValue("[]")
                  .WithColumn("OriginalFilePath").AsString().Nullable();

            Create.TableForModel("History")
                  .WithColumn("EpisodeId").AsInt32()
                  .WithColumn("SeriesId").AsInt32().Indexed()
                  .WithColumn("SourceTitle").AsString()
                  .WithColumn("Date").AsDateTimeOffset().Indexed()
                  .WithColumn("Quality").AsString()
                  .WithColumn("Data").AsString()
                  .WithColumn("EventType").AsInt32().Nullable().Indexed()
                  .WithColumn("DownloadId").AsString().Nullable()
                  .WithColumn("Languages").AsString().NotNullable().WithDefaultValue("[]");

            Create.Index().OnTable("History").OnColumn("EpisodeId").Ascending()
                                             .OnColumn("Date").Descending();

            Create.Index().OnTable("History").OnColumn("DownloadId").Ascending()
                                             .OnColumn("Date").Descending();

            Create.TableForModel("DownloadHistory")
                  .WithColumn("EventType").AsInt32().NotNullable().Indexed()
                  .WithColumn("SeriesId").AsInt32().NotNullable().Indexed()
                  .WithColumn("DownloadId").AsString().NotNullable().Indexed()
                  .WithColumn("SourceTitle").AsString().NotNullable()
                  .WithColumn("Date").AsDateTimeOffset().NotNullable()
                  .WithColumn("Protocol").AsInt32().Nullable()
                  .WithColumn("IndexerId").AsInt32().Nullable()
                  .WithColumn("DownloadClientId").AsInt32().Nullable()
                  .WithColumn("Release").AsString().Nullable()
                  .WithColumn("Data").AsString().Nullable();

            Create.TableForModel("Notifications")
                  .WithColumn("Name").AsString()
                  .WithColumn("OnGrab").AsBoolean()
                  .WithColumn("OnDownload").AsBoolean()
                  .WithColumn("Settings").AsString()
                  .WithColumn("Implementation").AsString()
                  .WithColumn("ConfigContract").AsString().Nullable()
                  .WithColumn("OnUpgrade").AsBoolean().Nullable()
                  .WithColumn("Tags").AsString().Nullable()
                  .WithColumn("OnRename").AsBoolean()
                  .WithColumn("OnHealthIssue").AsBoolean()
                  .WithColumn("IncludeHealthWarnings").AsBoolean().WithDefaultValue(false)
                  .WithColumn("OnSeriesDelete").AsBoolean().WithDefaultValue(false)
                  .WithColumn("OnEpisodeFileDelete").AsBoolean().WithDefaultValue(false)
                  .WithColumn("OnEpisodeFileDeleteForUpgrade").AsBoolean().WithDefaultValue(true)
                  .WithColumn("OnApplicationUpdate").AsBoolean().WithDefaultValue(false);

            Create.TableForModel("Blocklist")
                  .WithColumn("SeriesId").AsInt32().Indexed()
                  .WithColumn("EpisodeIds").AsString()
                  .WithColumn("SourceTitle").AsString()
                  .WithColumn("Quality").AsString()
                  .WithColumn("Date").AsDateTimeOffset()
                  .WithColumn("PublishedDate").AsDateTimeOffset().Nullable()
                  .WithColumn("Size").AsInt64().Nullable()
                  .WithColumn("Protocol").AsInt32().Nullable()
                  .WithColumn("Indexer").AsString().Nullable()
                  .WithColumn("Message").AsString().Nullable()
                  .WithColumn("TorrentInfoHash").AsString().Nullable()
                  .WithColumn("Languages").AsString().NotNullable().WithDefaultValue("[]");

            Create.TableForModel("ScheduledTasks")
                  .WithColumn("TypeName").AsString().Unique()
                  .WithColumn("Interval").AsInt32()
                  .WithColumn("LastExecution").AsDateTimeOffset()
                  .WithColumn("LastStartTime").AsDateTimeOffset().Nullable();

            Create.TableForModel("Indexers")
                  .WithColumn("EnableRss").AsBoolean().Nullable()
                  .WithColumn("EnableAutomaticSearch").AsBoolean()
                  .WithColumn("EnableInteractiveSearch").AsBoolean()
                  .WithColumn("Name").AsString().Unique()
                  .WithColumn("Implementation").AsString()
                  .WithColumn("Settings").AsString().Nullable()
                  .WithColumn("ConfigContract").AsString().Nullable()
                  .WithColumn("Priority").AsInt32().NotNullable().WithDefaultValue(25)
                  .WithColumn("Tags").AsString().Nullable()
                  .WithColumn("DownloadClientId").AsInt32().WithDefaultValue(0)
                  .WithColumn("SeasonSearchMaximumSingleEpisodeAge").AsInt32().NotNullable().WithDefaultValue(0);

            Create.TableForModel("IndexerStatus")
                  .WithColumn("ProviderId").AsInt32().NotNullable().Unique()
                  .WithColumn("InitialFailure").AsDateTimeOffset().Nullable()
                  .WithColumn("MostRecentFailure").AsDateTimeOffset().Nullable()
                  .WithColumn("EscalationLevel").AsInt32().NotNullable()
                  .WithColumn("DisabledTill").AsDateTimeOffset().Nullable()
                  .WithColumn("LastRssSyncReleaseInfo").AsString().Nullable();

            Create.TableForModel("DownloadClients")
                  .WithColumn("Enable").AsBoolean().NotNullable()
                  .WithColumn("Name").AsString().NotNullable()
                  .WithColumn("Implementation").AsString().NotNullable()
                  .WithColumn("Settings").AsString().NotNullable()
                  .WithColumn("ConfigContract").AsString().NotNullable()
                  .WithColumn("Priority").AsInt32().WithDefaultValue(1)
                  .WithColumn("RemoveCompletedDownloads").AsBoolean().NotNullable().WithDefaultValue(true)
                  .WithColumn("RemoveFailedDownloads").AsBoolean().NotNullable().WithDefaultValue(true);

            Create.TableForModel("DownloadClientStatus")
                  .WithColumn("ProviderId").AsInt32().NotNullable().Unique()
                  .WithColumn("InitialFailure").AsDateTimeOffset().Nullable()
                  .WithColumn("MostRecentFailure").AsDateTimeOffset().Nullable()
                  .WithColumn("EscalationLevel").AsInt32().NotNullable()
                  .WithColumn("DisabledTill").AsDateTimeOffset().Nullable();

            Create.TableForModel("QualityProfiles")
                  .WithColumn("Name").AsString().Unique()
                  .WithColumn("Cutoff").AsInt32()
                  .WithColumn("Items").AsString()
                  .WithColumn("UpgradeAllowed").AsBoolean().Nullable()
                  .WithColumn("FormatItems").AsString().WithDefaultValue("[]")
                  .WithColumn("MinFormatScore").AsInt32().WithDefaultValue(0)
                  .WithColumn("CutoffFormatScore").AsInt32().WithDefaultValue(0);

            Create.TableForModel("QualityDefinitions")
                  .WithColumn("Quality").AsInt32().Unique()
                  .WithColumn("Title").AsString().Unique()
                  .WithColumn("MinSize").AsDouble().Nullable()
                  .WithColumn("MaxSize").AsDouble().Nullable()
                  .WithColumn("PreferredSize").AsDouble().Nullable();

            Create.TableForModel("NamingConfig")
                  .WithColumn("MultiEpisodeStyle").AsInt32()
                  .WithColumn("RenameEpisodes").AsBoolean().Nullable()
                  .WithColumn("StandardEpisodeFormat").AsString().Nullable()
                  .WithColumn("SeasonFolderFormat").AsString().Nullable()
                  .WithColumn("SeriesFolderFormat").AsString().Nullable()
                  .WithColumn("ReplaceIllegalCharacters").AsBoolean().WithDefaultValue(true);

            Create.TableForModel("Metadata")
                  .WithColumn("Enable").AsBoolean().NotNullable()
                  .WithColumn("Name").AsString().NotNullable()
                  .WithColumn("Implementation").AsString().NotNullable()
                  .WithColumn("Settings").AsString().NotNullable()
                  .WithColumn("ConfigContract").AsString().NotNullable();

            Create.TableForModel("MetadataFiles")
                  .WithColumn("SeriesId").AsInt32().NotNullable()
                  .WithColumn("Consumer").AsString().NotNullable()
                  .WithColumn("Type").AsInt32().NotNullable()
                  .WithColumn("RelativePath").AsString().NotNullable()
                  .WithColumn("LastUpdated").AsDateTimeOffset().NotNullable()
                  .WithColumn("SeasonNumber").AsInt32().Nullable()
                  .WithColumn("EpisodeFileId").AsInt32().Nullable()
                  .WithColumn("Hash").AsString().Nullable()
                  .WithColumn("Added").AsDateTimeOffset().Nullable()
                  .WithColumn("Extension").AsString();

            Create.TableForModel("ExtraFiles")
                  .WithColumn("SeriesId").AsInt32().NotNullable()
                  .WithColumn("SeasonNumber").AsInt32().NotNullable()
                  .WithColumn("EpisodeFileId").AsInt32().NotNullable()
                  .WithColumn("RelativePath").AsString().NotNullable()
                  .WithColumn("Extension").AsString().NotNullable()
                  .WithColumn("Added").AsDateTimeOffset().NotNullable()
                  .WithColumn("LastUpdated").AsDateTimeOffset().NotNullable();

            Create.TableForModel("SubtitleFiles")
                  .WithColumn("SeriesId").AsInt32().NotNullable()
                  .WithColumn("SeasonNumber").AsInt32().NotNullable()
                  .WithColumn("EpisodeFileId").AsInt32().NotNullable()
                  .WithColumn("RelativePath").AsString().NotNullable()
                  .WithColumn("Extension").AsString().NotNullable()
                  .WithColumn("Added").AsDateTimeOffset().NotNullable()
                  .WithColumn("LastUpdated").AsDateTimeOffset().NotNullable()
                  .WithColumn("Language").AsInt32().NotNullable()
                  .WithColumn("LanguageTags").AsString().Nullable();

            Create.TableForModel("PendingReleases")
                  .WithColumn("SeriesId").AsInt32()
                  .WithColumn("Title").AsString()
                  .WithColumn("Added").AsDateTimeOffset()
                  .WithColumn("ParsedEpisodeInfo").AsString()
                  .WithColumn("Release").AsString()
                  .WithColumn("Reason").AsInt32().WithDefaultValue(0)
                  .WithColumn("AdditionalInfo").AsString().Nullable();

            Create.TableForModel("RemotePathMappings")
                  .WithColumn("Host").AsString()
                  .WithColumn("RemotePath").AsString()
                  .WithColumn("LocalPath").AsString();

            Create.TableForModel("Tags")
                  .WithColumn("Label").AsString().Unique();

            Create.TableForModel("DelayProfiles")
                  .WithColumn("EnableUsenet").AsBoolean().NotNullable()
                  .WithColumn("EnableTorrent").AsBoolean().NotNullable()
                  .WithColumn("PreferredProtocol").AsInt32().NotNullable()
                  .WithColumn("UsenetDelay").AsInt32().NotNullable()
                  .WithColumn("TorrentDelay").AsInt32().NotNullable()
                  .WithColumn("Order").AsInt32().NotNullable()
                  .WithColumn("Tags").AsString().NotNullable()
                  .WithColumn("BypassIfHighestQuality").AsBoolean().WithDefaultValue(false)
                  .WithColumn("BypassIfAboveCustomFormatScore").AsBoolean().WithDefaultValue(false)
                  .WithColumn("MinimumCustomFormatScore").AsInt32().Nullable();

            Create.TableForModel("Users")
                  .WithColumn("Identifier").AsString().NotNullable().Unique()
                  .WithColumn("Username").AsString().NotNullable().Unique()
                  .WithColumn("Password").AsString().NotNullable()
                  .WithColumn("Salt").AsString().Nullable()
                  .WithColumn("Iterations").AsInt32().Nullable();

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

            Create.TableForModel("CustomFilters")
                  .WithColumn("Type").AsString().NotNullable()
                  .WithColumn("Label").AsString().NotNullable()
                  .WithColumn("Filters").AsString().NotNullable();

            Create.TableForModel("ReleaseProfiles")
                  .WithColumn("Required").AsString().Nullable()
                  .WithColumn("Ignored").AsString().Nullable()
                  .WithColumn("Tags").AsString().NotNullable()
                  .WithColumn("Enabled").AsBoolean().WithDefaultValue(true)
                  .WithColumn("IndexerId").AsInt32().WithDefaultValue(0)
                  .WithColumn("Name").AsString().Nullable().WithDefaultValue(null);

            Create.TableForModel("ImportLists")
                  .WithColumn("Name").AsString().Unique()
                  .WithColumn("Implementation").AsString()
                  .WithColumn("Settings").AsString().Nullable()
                  .WithColumn("ConfigContract").AsString().Nullable()
                  .WithColumn("EnableAutomaticAdd").AsBoolean().Nullable()
                  .WithColumn("RootFolderPath").AsString()
                  .WithColumn("ShouldMonitor").AsInt32()
                  .WithColumn("QualityProfileId").AsInt32()
                  .WithColumn("Tags").AsString().Nullable()
                  .WithColumn("SeasonFolder").AsBoolean().WithDefaultValue(true);

            Create.TableForModel("ImportListStatus")
                  .WithColumn("ProviderId").AsInt32().NotNullable().Unique()
                  .WithColumn("InitialFailure").AsDateTimeOffset().Nullable()
                  .WithColumn("MostRecentFailure").AsDateTimeOffset().Nullable()
                  .WithColumn("EscalationLevel").AsInt32().NotNullable()
                  .WithColumn("DisabledTill").AsDateTimeOffset().Nullable()
                  .WithColumn("LastInfoSync").AsDateTimeOffset().Nullable();

            Create.TableForModel("ImportListExclusions")
                  .WithColumn("TvdbId").AsString().NotNullable().Unique()
                  .WithColumn("Title").AsString().NotNullable();

            Create.TableForModel("CustomFormats")
                  .WithColumn("Name").AsString().Unique()
                  .WithColumn("Specifications").AsString().WithDefaultValue("[]")
                  .WithColumn("IncludeCustomFormatWhenRenaming").AsBoolean().WithDefaultValue(false);

            Create.TableForModel("AutoTagging")
                  .WithColumn("Name").AsString().Unique()
                  .WithColumn("Specifications").AsString().WithDefaultValue("[]")
                  .WithColumn("RemoveTagsAutomatically").AsBoolean().WithDefaultValue(false)
                  .WithColumn("Tags").AsString().WithDefaultValue("[]");

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
