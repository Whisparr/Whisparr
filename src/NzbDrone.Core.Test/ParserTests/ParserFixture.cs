using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.ParserTests
{
    [TestFixture]
    public class ParserFixture : CoreTest
    {
        [TestCase("Series Title - 23-01-15 - Title", "seriestitle")]
        [TestCase("Series & Title - 23-01-15 - Title", "seriestitle")]
        [TestCase("Bad Format", "badformat")]
        [TestCase("Mad Series - 2014 [Bluray720p]", "madseries")]
        [TestCase("Mad Series - 2014 [Bluray1080p]", "madseries")]
        [TestCase("The Daily Series -", "thedailyseries")]
        [TestCase("The Series Bros. (2006)", "theseriesbros2006")]
        [TestCase("Series (2011)", "series2011")]
        [TestCase("Series Time 2013 720p HDTV x264 CRON", "seriestime")]
        [TestCase("Series Title 0", "seriestitle0")]
        [TestCase("Series of the Day", "seriesday")]
        [TestCase("Series of the Day 2", "seriesday2")]
        [TestCase("[ www.Torrenting.com ] - Series.23.01.23.720p.HDTV.X264-DIMENSION", "series")]
        [TestCase("www.Torrenting.com - Series.23.01.23.720p.HDTV.X264-DIMENSION", "series")]
        [TestCase("Series 2016-01-15 HDTV x264-2HD [eztv]-[rarbg.com]", "series")]
        [TestCase("Series.911.2013-2023.DVDRip.DD2.0.x264-DEEP", "series 911")]
        [TestCase("www.Torrenting.org - Series.23.01.23.720p.HDTV.X264-DIMENSION", "series")]
        public void should_parse_series_name(string postTitle, string title)
        {
            var result = Parser.Parser.ParseSeriesName(postTitle).CleanSeriesTitle();
            result.Should().Be(title.CleanSeriesTitle());
        }

        [TestCase("Series 23 01 23 720p HDTV X264-DIMENSION", "Series")]
        [TestCase("Series.23.01.23.720p.HDTV.X264-DIMENSION", "Series")]
        [TestCase("Series-23-01-23-720p-HDTV-X264-DIMENSION", "Series")]
        [TestCase("Series_23.01.23_720p_HDTV_X264-DIMENSION", "Series")]
        [TestCase("Series 2022 23 01 23 720p HDTV X264-DIMENSION", "Series", 2022)]
        [TestCase("Series (2022) 23 01 23 720p HDTV X264-DIMENSION", "Series", 2022)]
        [TestCase("Series.2022.23.01.23.720p.HDTV.X264-DIMENSION", "Series", 2022)]
        [TestCase("Series-2022-23-01-23-720p-HDTV-X264-DIMENSION", "Series", 2022)]
        [TestCase("Series_2022_23_01_23_720p_HDTV_X264-DIMENSION", "Series", 2022)]
        [TestCase("1234 23 01 23 720p HDTV X264-DIMENSION", "1234")]
        [TestCase("1234.23.01.23.720p.HDTV.X264-DIMENSION", "1234")]
        [TestCase("1234-23-01-23-720p-HDTV-X264-DIMENSION", "1234")]
        [TestCase("1234_23_01_23_720p_HDTV_X264-DIMENSION", "1234")]
        [TestCase("1234 2022 23 01 23 720p HDTV X264-DIMENSION", "1234", 2022)]
        [TestCase("1234 (2022) 23 01 23 720p HDTV X264-DIMENSION", "1234", 2022)]
        [TestCase("1234.2022.23.01.23.720p.HDTV.X264-DIMENSION", "1234", 2022)]
        [TestCase("1234-2022-23-01-23-720p-HDTV-X264-DIMENSION", "1234", 2022)]
        [TestCase("1234_2022_23_01_23_720p_HDTV_X264-DIMENSION", "1234", 2022)]
        public void should_parse_series_title_info(string postTitle, string titleWithoutYear, int year = 0)
        {
            var seriesTitleInfo = Parser.Parser.ParseTitle(postTitle).SeriesTitleInfo;
            seriesTitleInfo.TitleWithoutYear.Should().Be(titleWithoutYear);
            seriesTitleInfo.Year.Should().Be(year);
        }

        [Test]
        public void should_remove_accents_from_title()
        {
            const string title = "Seri\u00E0es";

            title.CleanSeriesTitle().Should().Be("seriaes");
        }

        [TestCase("Sonar TV - Series Title : 02 Road From Code [S04].mp4")]
        public void should_clean_up_invalid_path_characters(string postTitle)
        {
            Parser.Parser.ParseTitle(postTitle);
        }

        [TestCase("[scnzbefnet][509103] 2.Developers.Series.23-01-22.720p.HDTV.X264-DIMENSION", "2 Developers Series")]
        public void should_remove_request_info_from_title(string postTitle, string title)
        {
            Parser.Parser.ParseTitle(postTitle).SeriesTitle.Should().Be(title);
        }

        [TestCase("Series.23.01.23.Chained.Title.mkv")]
        [TestCase("Show - 23.01.23 - Title.avi")]
        public void should_parse_quality_from_extension(string title)
        {
            Parser.Parser.ParseTitle(title).Quality.Quality.Should().NotBe(Quality.Unknown);
            Parser.Parser.ParseTitle(title).Quality.SourceDetectionSource.Should().Be(QualityDetectionSource.Extension);
            Parser.Parser.ParseTitle(title).Quality.ResolutionDetectionSource.Should().Be(QualityDetectionSource.Extension);
        }

        [TestCase("Series.23.01.23.Chained.Title.mkv", "Series.23.01.23.Chained.Title")]
        public void should_parse_releasetitle(string path, string releaseTitle)
        {
            var result = Parser.Parser.ParseTitle(path);
            result.ReleaseTitle.Should().Be(releaseTitle);
        }
    }
}
