using System;
using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Cloud;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Update;

namespace NzbDrone.Core.Test.UpdateTests
{
    public class UpdatePackageProviderFixture : CoreTest<GithubUpdatePackageProvider>
    {
        [SetUp]
        public void Setup()
        {
            // Mock IWhisparrCloudRequestBuilder to ensure GithubReleases.Create() returns a working builder
            var realBuilder = new HttpRequestBuilder("https://api.github.com/repos/{githubownerrepo}/releases");
            var mockBuilderFactory = new Mock<IHttpRequestBuilderFactory>();
            mockBuilderFactory.Setup(f => f.Create()).Returns(realBuilder);
            var mockCloudRequestBuilder = new Mock<IWhisparrCloudRequestBuilder>();
            mockCloudRequestBuilder.SetupGet(c => c.GithubReleases).Returns(mockBuilderFactory.Object);
            Mocker.SetConstant<IWhisparrCloudRequestBuilder>(mockCloudRequestBuilder.Object);

            Mocker.GetMock<IPlatformInfo>().SetupGet(c => c.Version).Returns(new Version("9.9.9"));
            Mocker.GetMock<IConfigFileProvider>()
                .SetupGet(c => c.GithubOwnerRepo)
                .Returns("whisparr/whisparr");

            // Mock IHttpClient to return a simulated GitHub releases API response from file
            var testDataPath = System.IO.Path.Combine(AppContext.BaseDirectory, "UpdateTests", "TestData", "GithubReleasesResponse.json");
            var fakeResponse = System.IO.File.ReadAllText(testDataPath);
            var request = new HttpRequest("https://api.github.com/repos/whisparr/whisparr/releases?per_page=25");
            var headers = new HttpHeader();
            var httpResponse = new HttpResponse(request, headers, fakeResponse, System.Net.HttpStatusCode.OK);
            Mocker.GetMock<IHttpClient>()
                .Setup(c => c.Get(It.IsAny<HttpRequest>()))
                .Returns(httpResponse);
        }

        [Test]
        public void no_update_when_version_higher()
        {
            Subject.GetLatestUpdate("v2", new Version(10, 0, 0)).Should().BeNull();
        }

        [Test]
        public void finds_update_when_version_lower()
        {
            Subject.GetLatestUpdate("v2", new Version(2, 0, 0)).Should().NotBeNull();
        }

        [Test]
        public void should_get_nothing_if_branch_doesnt_exit()
        {
            Subject.GetLatestUpdate("invalid_branch", new Version(3, 0)).Should().BeNull();
        }

        // Installs created before the branch default was corrected persisted Sonarr's
        // branch names. Those matched neither arm of the old filter, so nothing was
        // filtered and a stable install was offered the newest build overall -- always
        // a develop one. An unknown branch must be treated as stable.
        [TestCase("nightly")]
        [TestCase("master")]
        [TestCase("")]
        public void should_not_offer_develop_builds_to_unrecognised_branches(string branch)
        {
            var recent = Subject.GetRecentUpdates(branch, new Version(1, 0), null);

            recent.Should().NotBeEmpty();
            recent.Should().OnlyContain(c => !c.FileName.Contains("develop"));
        }

        // A branch that names the develop channel still selects pre-releases, even
        // though it is not one of Whisparr's own branch names. ConfigFileProvider
        // normalises stored values before they reach here, so this only covers a
        // branch passed in directly.
        [TestCase("develop")]
        [TestCase("v2-develop")]
        public void should_offer_develop_builds_to_develop_branches(string branch)
        {
            var recent = Subject.GetRecentUpdates(branch, new Version(1, 0), null);

            recent.Should().NotBeEmpty();
            recent.Should().OnlyContain(c => c.FileName.Contains("develop"));
        }

        [Test]
        public void should_not_offer_a_develop_build_as_the_latest_update_for_a_legacy_branch()
        {
            var update = Subject.GetLatestUpdate("nightly", new Version(1, 0));

            update.Should().NotBeNull();
            update.FileName.Should().NotContain("develop");
        }

        [Test]
        public void should_get_recent_updates_for_stable_branch()
        {
            const string branch = "v2";
            var recent = Subject.GetRecentUpdates(branch, new Version(2, 0), null);
            var recentWithChanges = recent.Where(c => c.Changes != null);

            recent.Should().NotBeEmpty();
            recent.Should().OnlyContain(c => c.Hash.IsNotNullOrWhiteSpace());
            recent.Should().OnlyContain(c => c.FileName.Contains("Whisparr"));
            recent.Should().OnlyContain(c => c.ReleaseDate.Year >= 2014);

            // Should only contain release versions, not develop
            recent.Should().OnlyContain(c => !c.FileName.Contains("develop"));

            if (recentWithChanges.Any())
            {
                recentWithChanges.Should().OnlyContain(c => c.Changes.New != null);
            }

            recent.Should().OnlyContain(c => c.Branch == branch);
        }

        [Test]
        public void should_get_recent_updates_for_develop_branch()
        {
            const string branch = "v2-develop";
            var recent = Subject.GetRecentUpdates(branch, new Version(2, 0), null);
            var recentWithChanges = recent.Where(c => c.Changes != null);

            recent.Should().NotBeEmpty();
            recent.Should().OnlyContain(c => c.Hash.IsNotNullOrWhiteSpace());
            recent.Should().OnlyContain(c => c.FileName.Contains("Whisparr"));
            recent.Should().OnlyContain(c => c.ReleaseDate.Year >= 2014);

            // Should only contain develop versions, not release
            recent.Should().OnlyContain(c => c.FileName.Contains("develop"));

            if (recentWithChanges.Any())
            {
                recentWithChanges.Should().OnlyContain(c => c.Changes.New != null);
            }

            recent.Should().OnlyContain(c => c.Branch == branch);
        }

        [Test]
        public void should_filter_stable_releases_for_v2_branch()
        {
            var recent = Subject.GetRecentUpdates("v2", new Version(1, 0), null);

            // Should have 2 stable releases (v2.1.0-release.50 and v2.0.0-release.25)
            recent.Should().HaveCount(2);
            recent.Should().OnlyContain(c => c.Version.Prerelease.Contains("release"));
        }

        [Test]
        public void should_filter_develop_releases_for_v2_develop_branch()
        {
            var recent = Subject.GetRecentUpdates("v2-develop", new Version(1, 0), null);

            // Should have 2 develop releases (v2.1.0-develop.100 and v2.0.0-develop.50)
            recent.Should().HaveCount(2);
            recent.Should().OnlyContain(c => c.Version.Prerelease.Contains("develop"));
        }
    }
}
