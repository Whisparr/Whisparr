using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(008)]
    public class add_credits : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Create.TableForModel("Credits").WithColumn("MovieMetadataId").AsInt32()
                                  .WithColumn("PerformerForeignId").AsString()
                                  .WithColumn("Character").AsString().Nullable()
                                  .WithColumn("Job").AsString().Nullable()
                                  .WithColumn("Type").AsInt32()
                                  .WithColumn("Order").AsInt32();

            // Indexes
            Create.Index().OnTable("Credits").OnColumn("MovieMetadataId");
            Create.Index().OnTable("Credits").OnColumn("PerformerForeignId");
        }
    }
}
