using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Dapper;
using FluentMigrator;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(999)]
    public class add_performers_table : NzbDroneMigrationBase
    {
        private readonly JsonSerializerOptions _serializerSettings;

        public add_performers_table()
        {
            _serializerSettings = new JsonSerializerOptions
            {
                AllowTrailingCommas = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                PropertyNameCaseInsensitive = true,
                DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };
        }

        protected override void MainDbUpgrade()
        {
            Create.TableForModel("Performers")
                .WithColumn("TpdbId").AsInt32().Unique()
                .WithColumn("Name").AsString()
                .WithColumn("Character").AsString()
                .WithColumn("Gender").AsString()
                .WithColumn("Images").AsString().WithDefaultValue("[]");

            Alter.Table("Episodes").AddColumn("Performers").AsString().WithDefaultValue("[]");

            Execute.WithConnection(UniqueActors);

            // TODO: Add back in once run through Performers with ActorID
            // Delete.Column("Actors").FromTable("Episodes");
            // Rename.Column("Performers").OnTable("Episodes").To("Actors");
        }

        private void UniqueActors(IDbConnection conn, IDbTransaction tran)
        {
            var actors = new List<Actor11Serialized>();
            using (var actorsCmd = conn.CreateCommand())
            {
                actorsCmd.Transaction = tran;
                actorsCmd.CommandText = "SELECT \"Actors\" FROM \"Episodes\"";

                using (var reader = actorsCmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var data = STJson.Deserialize<List<Actor11>>(reader.GetString(0));
                        foreach (var actor in data)
                        {
                            actors.Add(new Actor11Serialized
                            {
                                TpdbId = actor.TpdbId,
                                Name = actor.Name,
                                Character = actor.Character,
                                Gender = actor.Gender,
                                Images = JsonSerializer.Serialize(actor.Images, _serializerSettings)
                            });
                        }
                    }
                }

                var updatedActors = actors.GroupBy(x => x.TpdbId).Select(x => x.First()).ToList();

                // Currently not working - to investigate.
                var updateSql = "UPDATE \"Performers\" SET \"TpdbId\" = @TpdbId, \"Name\" = @Name, \"Character\" = @Character, \"Gender\" = @Gender, \"Images\" = @Images";
                conn.Execute(updateSql, updatedActors, transaction: tran);
            }
        }

        private class Actor11
        {
            public int TpdbId { get; set; }
            public string Name { get; set; }
            public string Character { get; set; }
            public string Gender { get; set; }
            public List<MediaCover11> Images { get; set; }
        }

        private class Actor11Serialized
        {
            public int TpdbId { get; set; }
            public string Name { get; set; }
            public string Character { get; set; }
            public string Gender { get; set; }
            public string Images { get; set; }
        }

        private class MediaCover11
        {
            public MediaCoverTypes11 CoverType { get; set; }
            public string Url { get; set; }
            public string RemoteUrl { get; set; }

            public MediaCover11()
            {
            }

            public MediaCover11(MediaCoverTypes11 coverType, string remoteUrl)
            {
                CoverType = coverType;
                RemoteUrl = remoteUrl;
            }
        }

        public enum MediaCoverTypes11
        {
            Unknown = 0,
            Logo = 1,
            Poster = 2,
            Banner = 3,
            Fanart = 4,
            Screenshot = 5,
            Headshot = 6,
            Clearlogo = 7
        }
    }
}
