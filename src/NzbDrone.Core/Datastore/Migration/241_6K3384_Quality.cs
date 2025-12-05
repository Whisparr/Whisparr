using System.Data;
using FluentMigrator;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(241)]
    public class AddHighWebDlQualities3384 : AddHighWebDlQualities
    {
        protected override void MainDbUpgrade()
        {
            Execute.WithConnection(PerformHighWebDlQualityMigration);
        }

        private void PerformHighWebDlQualityMigration(IDbConnection conn, IDbTransaction tran)
        {
            var updater = new ProfileUpdater125(conn, tran);
            updater.SplitQualityAppend(10, 33); // WEBDL-2880p
            updater.SplitQualityAppend(33, 34); // WEBDL-3160p
            updater.SplitQualityAppend(34, 36); // WEBDL-3384p
            updater.SplitQualityAppend(36, 35); // WEBDL-4320p

            updater.Commit();
        }
    }
}
