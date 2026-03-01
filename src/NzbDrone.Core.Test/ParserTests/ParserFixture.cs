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

        // [TestCase("Mad Series - 2014 [Bluray720p]", "madseries")]
        // [TestCase("Mad Series - 2014 [Bluray1080p]", "madseries")]
        [TestCase("The Daily Series -", "thedailyseries")]
        [TestCase("The Series Bros. (2006)", "theseriesbros2006")]
        [TestCase("Series (2011)", "series2011")]

        // [TestCase("Series Time 2013 720p HDTV x264 CRON", "seriestime")]
        [TestCase("Series Title 0", "seriestitle0")]
        [TestCase("Series of the Day", "seriesday")]
        [TestCase("Series of the Day 2", "seriesday2")]
        [TestCase("[ www.Torrenting.com ] - Series.23.01.23.720p.HDTV.X264-DIMENSION", "series")]
        [TestCase("www.Torrenting.com - Series.23.01.23.720p.HDTV.X264-DIMENSION", "series")]
        [TestCase("Series 2016-01-15 HDTV x264-2HD [eztv]-[rarbg.com]", "series")]

        // [TestCase("Series.911.2023.DVDRip.DD2.0.x264-DEEP", "series 911")]
        [TestCase("www.Torrenting.org - Series.23.01.23.720p.HDTV.X264-DIMENSION", "series")]
        [TestCase("Pure Taboo - Sarah Arabic, Lily LaBeau - A Costly Divorce (June 24, 2025) [1080p HEVC x265]", "puretaboo")]
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
        [TestCase("Pure Taboo - Sarah Arabic, Lily LaBeau - A Costly Divorce (June 24, 2025) [1080p HEVC x265]", "Pure Taboo")]
        public void should_parse_series_title_info(string postTitle, string titleWithoutYear, int year = 0)
        {
            var seriesTitleInfo = Parser.Parser.ParseTitle(postTitle).SeriesTitleInfo;
            seriesTitleInfo.TitleWithoutYear.Should().Be(titleWithoutYear);
            seriesTitleInfo.Year.Should().Be(year);
        }

        [TestCase("Digital Playground - 2014-12-20 - Dirty Santa - Episode 4 - Candy Cane Lane - [WEBDL-1080p].mp4", " - Dirty Santa - Episode 4 - Candy Cane Lane - ")]
        [TestCase("Pure Taboo - Sarah Arabic, Lily LaBeau - A Costly Divorce (June 24, 2025) [1080p HEVC x265]", " - Sarah Arabic, Lily LaBeau - A Costly Divorce")]
        public void should_parse_episode_string(string title, string expected)
        {
            var seriesTitleInfo = Parser.Parser.ParseTitle(title);
            if (title.Contains("2014-12-20"))
            {
                seriesTitleInfo.AirDate.Should().Be("2014-12-20");
            }
            else if (title.Contains("June 24, 2025"))
            {
                seriesTitleInfo.AirDate.Should().Be("2025-06-24");
            }

            seriesTitleInfo.ReleaseTokens.Should().Be(expected);
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
        [TestCase("Show - 23.01.23 - Title.f4v")]
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

        [TestCase("MIKR-058.mp4", "MIKR-058")]
        [TestCase("ABC-123.mkv", "ABC-123")]
        [TestCase("XYZ_456.avi", "XYZ_456")]
        [TestCase("ABCD-1234", "ABCD-1234")]
        [TestCase("AB-12.mp4", "AB-12")]
        [TestCase("ABCDEFGHIJ-1234567890", "ABCDEFGHIJ-1234567890")]
        [TestCase("hhd800.com@NFD-039.mp4", "NFD-039")]
        [TestCase("site.org@ABC-123.mkv", "ABC-123")]
        public void should_parse_external_id_from_filename(string filename, string expectedExternalId)
        {
            var result = Parser.Parser.ParseExternalIdFromFilename(filename);
            result.Should().Be(expectedExternalId);
        }

        [TestCase("Some Random Title.mp4")]
        [TestCase("Series.23.01.23.Title.mkv")]
        [TestCase("A-1.mp4")] // Too short - needs 2+ chars on each side
        [TestCase("ABCDEFGHIJK-12345678901")] // Too long - max 10 chars on each side
        [TestCase("123-ABC.mp4")] // First part must be letters only
        [TestCase("Hands-On Something.mp4")] // Series name, no digits in second component
        [TestCase("Come-In Title.mp4")] // Series name, no digits in second component
        [TestCase("Stand-By Me.mp4")] // Series name, no digits in second component
        public void should_not_parse_external_id_from_non_matching_filename(string filename)
        {
            var result = Parser.Parser.ParseExternalIdFromFilename(filename);
            result.Should().BeNull();
        }

        [TestCase("[MIKR-058] Some Title", "MIKR-058")]
        [TestCase("[ABC-123]", "ABC-123")]
        [TestCase("[XYZ_456] Title Here", "XYZ_456")]
        public void should_parse_external_id_from_bracketed_title(string title, string expectedExternalId)
        {
            var result = Parser.Parser.ParseExternalId(title);
            result.Should().Be(expectedExternalId);
        }

        [TestCase("Some Title Without Bracket")]
        [TestCase("[SubGroup] Some Title")]
        [TestCase("Hands-On [01+22] - Mario Gets A Very Hands-On Massage")] // Exact failing title from bug report
        public void should_not_parse_external_id_from_non_matching_title(string title)
        {
            var result = Parser.Parser.ParseExternalId(title);
            result.Should().BeNull();
        }

        [TestCase("MIKR-058 Without Bracket", "MIKR-058")]
        [TestCase("MIAA-521 The Absolute Domain Some Title Here.mp4", "MIAA-521")]
        public void should_parse_external_id_from_title_with_space(string title, string expectedExternalId)
        {
            var result = Parser.Parser.ParseExternalId(title);
            result.Should().Be(expectedExternalId);
        }
    }
}
