using System.IO;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.OrganizerTests
{
    [TestFixture]

    public class GetSeriesFolderFixture : CoreTest<FileNameBuilder>
    {
        private NamingConfig _namingConfig;

        [SetUp]
        public void Setup()
        {
            _namingConfig = NamingConfig.Default;

            Mocker.GetMock<INamingConfigService>()
                  .Setup(c => c.GetConfig()).Returns(_namingConfig);
        }

        [TestCase("30 Rock", "{Site Title}", "30 Rock")]
        [TestCase("30 Rock", "{Site.Title}", "30.Rock")]
        [TestCase("24/7 Road to the NHL Winter Classic", "{Site Title}", "24+7 Road to the NHL Winter Classic")]
        [TestCase("Venture Bros.", "{Site.Title}", "Venture.Bros")]
        [TestCase(".hack", "{Site.Title}", "hack")]
        [TestCase("30 Rock", ".{Site.Title}.", "30.Rock")]
        public void should_use_seriesFolderFormat_to_build_folder_name(string seriesTitle, string format, string expected)
        {
            _namingConfig.SeriesFolderFormat = format;

            var series = new Series { Title = seriesTitle };

            Subject.GetSeriesFolder(series).Should().Be(expected);
        }

        [TestCase("HBO", "30 Rock", "HBO", "30 Rock")]
        [TestCase("", "Venture Bros", "Venture Bros")]
        public void should_use_site_network_in_seriesFolderFormat_to_build_folder_name(string seriesNetwork, string seriesTitle, params string[] expected)
        {
            _namingConfig.SeriesFolderFormat = "{Site Network}\\{Site Title}";

            var series = new Series { Network = seriesNetwork, Title = seriesTitle };

            Subject.GetSeriesFolder(series).Should().Be(Path.Combine(expected));
        }
    }
}
