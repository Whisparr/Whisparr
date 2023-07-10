using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.ParserTests
{
    [TestFixture]
    public class SingleEpisodeParserFixture : CoreTest
    {
        [TestCase("Series 23 01 23 720p HDTV X264-DIMENSION", "Series", "2023-01-23")]
        [TestCase("Series.23.01.23.720p.HDTV.X264-DIMENSION", "Series", "2023-01-23")]
        [TestCase("Series-23-01-23-720p-HDTV-X264-DIMENSION", "Series", "2023-01-23")]
        [TestCase("Series_23.01.23_720p_HDTV_X264-DIMENSION", "Series", "2023-01-23")]
        [TestCase("Series 2022 23 01 23 720p HDTV X264-DIMENSION", "Series 2022", "2023-01-23")]

        // [TestCase("Series (2022) 23 01 23 720p HDTV X264-DIMENSION", "Series (2022)", "2023-01-23")]
        [TestCase("Series.2022.23.01.23.720p.HDTV.X264-DIMENSION", "Series 2022", "2023-01-23")]

        // [TestCase("Series-2022-23-01-23-720p-HDTV-X264-DIMENSION", "Series 2022", "2023-01-23")]
        [TestCase("Series_2022_23_01_23_720p_HDTV_X264-DIMENSION", "Series 2022", "2023-01-23")]
        [TestCase("1234 23 01 23 720p HDTV X264-DIMENSION", "1234", "2023-01-23")]
        [TestCase("1234.23.01.23.720p.HDTV.X264-DIMENSION", "1234", "2023-01-23")]
        [TestCase("1234-23-01-23-720p-HDTV-X264-DIMENSION", "1234", "2023-01-23")]
        [TestCase("1234_23_01_23_720p_HDTV_X264-DIMENSION", "1234", "2023-01-23")]
        [TestCase("1234 2022 23 01 23 720p HDTV X264-DIMENSION", "1234 2022", "2023-01-23")]

        // [TestCase("1234 (2022) 23 01 23 720p HDTV X264-DIMENSION", "1234 (2022)", "2023-01-23")]
        [TestCase("1234.2022.23.01.23.720p.HDTV.X264-DIMENSION", "1234 2022", "2023-01-23")]

        // [TestCase("1234-2022-23-01-23-720p-HDTV-X264-DIMENSION", "1234 2022", "2023-01-23")]
        [TestCase("1234_2022_23_01_23_720p_HDTV_X264-DIMENSION", "1234 2022", "2023-01-23")]

        [TestCase("Series Title - 2013-05-15 - 36 Ciara [WEBDL-1080p]", "Series Title", "2013-05-15")]
        public void should_parse_single_episode(string postTitle, string title, string airDate)
        {
            var result = Parser.Parser.ParseTitle(postTitle);
            result.Should().NotBeNull();
            result.SeriesTitle.Should().Be(title);
            result.AirDate.Should().Be(airDate);
        }
    }
}
