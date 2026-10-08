using System;
using System.Collections.Generic;
using FluentAssertions;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using NzbDrone.Core.Movies.Performers;
using NzbDrone.Core.Notifications.Stash;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.NotificationTests.Stash
{
    [TestFixture]
    public class StashServicePerformerSyncFixture : CoreTest<StashService>
    {
        private const string ForeignId = "274ff928-2a83-4f0b-a3d9-0a1a3f47eb16";
        private StashSettings _settings;

        [SetUp]
        public void Setup()
        {
            _settings = new StashSettings
            {
                PerformerSyncMode = StashPerformerSyncMode.Bidirectional,
                PerformerSyncRootFolderPath = "/library",
                PerformerSyncQualityProfileId = 1
            };

            GivenWhisparrPerformers(new List<Performer>());
            GivenStashPerformers(new List<StashPerformer>());
        }

        [Test]
        public void should_favorite_in_stash_when_initial_whisparr_performer_is_monitored()
        {
            GivenWhisparrPerformers(new List<Performer> { WhisparrPerformer(true) });
            GivenStashPerformers(new List<StashPerformer> { StashPerformer(false) });
            GivenFavoriteUpdate(true);

            var result = Subject.SyncPerformers(_settings);

            Mocker.GetMock<IStashProxy>()
                .Verify(proxy => proxy.UpdatePerformerFavorite(_settings, "10", true), Times.Once);
            result.Updated.Should().Be(1);
            ReadState()[ForeignId].StashFavorite.Should().BeTrue();
        }

        [Test]
        public void should_unmonitor_in_whisparr_when_stash_favorite_is_removed()
        {
            var performer = WhisparrPerformer(true);
            GivenWhisparrPerformers(new List<Performer> { performer });
            GivenStashPerformers(new List<StashPerformer> { StashPerformer(false) });
            GivenState(new StashPerformerSyncState
            {
                StashFavorite = true,
                WhisparrMonitored = true,
                StashId = "10",
                WhisparrId = performer.Id
            });

            Subject.SyncPerformers(_settings);

            Mocker.GetMock<IPerformerService>()
                .Verify(service => service.Update(It.Is<Performer>(updated => !updated.Monitored)), Times.Once);
            Mocker.GetMock<IStashProxy>()
                .Verify(proxy => proxy.UpdatePerformerFavorite(It.IsAny<StashSettings>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
        }

        [Test]
        public void should_unfavorite_in_stash_when_whisparr_monitoring_is_removed()
        {
            var performer = WhisparrPerformer(false);
            GivenWhisparrPerformers(new List<Performer> { performer });
            GivenStashPerformers(new List<StashPerformer> { StashPerformer(true) });
            GivenFavoriteUpdate(false);
            GivenState(new StashPerformerSyncState
            {
                StashFavorite = true,
                WhisparrMonitored = true,
                StashId = "10",
                WhisparrId = performer.Id
            });

            Subject.SyncPerformers(_settings);

            Mocker.GetMock<IStashProxy>()
                .Verify(proxy => proxy.UpdatePerformerFavorite(_settings, "10", false), Times.Once);
            Mocker.GetMock<IPerformerService>()
                .Verify(service => service.Update(It.IsAny<Performer>()), Times.Never);
        }

        [Test]
        public void should_monitor_in_whisparr_when_stash_favorite_is_added()
        {
            var performer = WhisparrPerformer(false);
            GivenWhisparrPerformers(new List<Performer> { performer });
            GivenStashPerformers(new List<StashPerformer> { StashPerformer(true) });
            GivenState(new StashPerformerSyncState
            {
                StashFavorite = false,
                WhisparrMonitored = false,
                StashId = "10",
                WhisparrId = performer.Id
            });

            Subject.SyncPerformers(_settings);

            Mocker.GetMock<IPerformerService>()
                .Verify(service => service.Update(It.Is<Performer>(updated => updated.Monitored)), Times.Once);
            Mocker.GetMock<IStashProxy>()
                .Verify(proxy => proxy.UpdatePerformerFavorite(It.IsAny<StashSettings>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
        }

        [Test]
        public void should_create_whisparr_performer_from_stash_favorite_with_safe_defaults()
        {
            GivenStashPerformers(new List<StashPerformer> { StashPerformer(true) });
            Mocker.GetMock<IAddPerformerService>()
                .Setup(service => service.AddPerformer(It.IsAny<Performer>(), false))
                .Returns<Performer, bool>((performer, _) =>
                {
                    performer.Id = 22;
                    return performer;
                });

            Subject.SyncPerformers(_settings);

            Mocker.GetMock<IAddPerformerService>()
                .Verify(service => service.AddPerformer(It.Is<Performer>(performer =>
                    performer.ForeignId == ForeignId &&
                    performer.Monitored &&
                    !performer.MoviesMonitored &&
                    !performer.SearchOnAdd &&
                    performer.RootFolderPath == "/library" &&
                    performer.QualityProfileId == 1), false), Times.Once);
        }

        [Test]
        public void should_create_favorite_stash_performer_from_monitored_whisparr_performer()
        {
            GivenWhisparrPerformers(new List<Performer> { WhisparrPerformer(true) });
            Mocker.GetMock<IStashProxy>()
                .Setup(proxy => proxy.CreatePerformer(_settings, "Test Performer", ForeignId))
                .Returns(StashPerformer(true));

            Subject.SyncPerformers(_settings);

            Mocker.GetMock<IStashProxy>()
                .Verify(proxy => proxy.CreatePerformer(_settings, "Test Performer", ForeignId), Times.Once);
        }

        [Test]
        public void should_be_idempotent_when_both_sides_match_saved_state()
        {
            var performer = WhisparrPerformer(true);
            GivenWhisparrPerformers(new List<Performer> { performer });
            GivenStashPerformers(new List<StashPerformer> { StashPerformer(true) });
            GivenState(new StashPerformerSyncState
            {
                StashFavorite = true,
                WhisparrMonitored = true,
                StashId = "10",
                WhisparrId = performer.Id
            });

            var result = Subject.SyncPerformers(_settings);

            result.Updated.Should().Be(0);
            result.Created.Should().Be(0);
            Mocker.GetMock<IPerformerService>().Verify(service => service.Update(It.IsAny<Performer>()), Times.Never);
            Mocker.GetMock<IStashProxy>().Verify(proxy => proxy.UpdatePerformerFavorite(It.IsAny<StashSettings>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
        }

        [Test]
        public void should_not_write_when_stash_inventory_read_fails()
        {
            GivenWhisparrPerformers(new List<Performer> { WhisparrPerformer(true) });
            Mocker.GetMock<IStashProxy>()
                .Setup(proxy => proxy.GetPerformers(_settings))
                .Throws(new InvalidOperationException("unavailable"));

            Assert.Throws<InvalidOperationException>(() => Subject.SyncPerformers(_settings));

            Mocker.GetMock<IPerformerService>().Verify(service => service.Update(It.IsAny<Performer>()), Times.Never);
            Mocker.GetMock<IStashProxy>().Verify(proxy => proxy.CreatePerformer(It.IsAny<StashSettings>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public void should_skip_duplicate_stash_identities()
        {
            GivenWhisparrPerformers(new List<Performer> { WhisparrPerformer(true) });
            GivenStashPerformers(new List<StashPerformer>
            {
                StashPerformer(false, "10"),
                StashPerformer(false, "11")
            });

            var result = Subject.SyncPerformers(_settings);

            result.Warnings.Should().Be(1);
            Mocker.GetMock<IStashProxy>().Verify(proxy => proxy.UpdatePerformerFavorite(It.IsAny<StashSettings>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
        }

        [Test]
        public void should_prefer_removal_for_incompatible_changes()
        {
            var target = StashService.ResolveTarget(
                StashPerformerSyncMode.Bidirectional,
                new StashPerformerSyncState { StashFavorite = true, WhisparrMonitored = false },
                false,
                true);

            target.Should().BeFalse();
        }

        private void GivenWhisparrPerformers(List<Performer> performers)
        {
            Mocker.GetMock<IPerformerService>()
                .Setup(service => service.GetAllPerformers())
                .Returns(performers);
        }

        private void GivenStashPerformers(List<StashPerformer> performers)
        {
            Mocker.GetMock<IStashProxy>()
                .Setup(proxy => proxy.GetPerformers(_settings))
                .Returns(performers);
        }

        private void GivenFavoriteUpdate(bool favorite)
        {
            Mocker.GetMock<IStashProxy>()
                .Setup(proxy => proxy.UpdatePerformerFavorite(_settings, "10", favorite))
                .Returns(StashPerformer(favorite));
        }

        private void GivenState(StashPerformerSyncState state)
        {
            _settings.PerformerSyncState = JsonConvert.SerializeObject(new Dictionary<string, StashPerformerSyncState>
            {
                [ForeignId] = state
            });
        }

        private Dictionary<string, StashPerformerSyncState> ReadState()
        {
            return JsonConvert.DeserializeObject<Dictionary<string, StashPerformerSyncState>>(_settings.PerformerSyncState);
        }

        private static Performer WhisparrPerformer(bool monitored)
        {
            return new Performer
            {
                Id = 20,
                ForeignId = ForeignId,
                Name = "Test Performer",
                Monitored = monitored,
                RootFolderPath = "/library",
                QualityProfileId = 1
            };
        }

        private static StashPerformer StashPerformer(bool favorite, string id = "10")
        {
            return new StashPerformer
            {
                Id = id,
                Name = "Test Performer",
                Favorite = favorite,
                StashIds = new List<StashPerformerId>
                {
                    new StashPerformerId
                    {
                        Endpoint = StashProxy.StashDbEndpoint,
                        StashId = ForeignId
                    }
                }
            };
        }
    }
}
