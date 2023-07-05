using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(8)]
    public class clean_episode_model : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            // Clean up some things
            Delete.Column("EpisodeNumber").FromTable("Episodes");
            Delete.Column("SceneAbsoluteEpisodeNumber").FromTable("Episodes");
            Delete.Column("SceneSeasonNumber").FromTable("Episodes");
            Delete.Column("SceneEpisodeNumber").FromTable("Episodes");
            Delete.Column("AiredAfterSeasonNumber").FromTable("Episodes");
            Delete.Column("AiredBeforeSeasonNumber").FromTable("Episodes");
            Delete.Column("AiredBeforeEpisodeNumber").FromTable("Episodes");
            Delete.Column("UnverifiedSceneNumbering").FromTable("Episodes");
        }
    }
}
