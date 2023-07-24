using System;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.Expansive;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.ParserTests
{
    [TestFixture]
    public class DailyEpisodeParserFixture : CoreTest
    {
        [TestCase("Series Title 2011 04 18 Emma Roberts HDTV XviD BFF", "Series Title", 2011, 04, 18)]
        [TestCase("A Late Talk Show 2011 04 15 1080i HDTV DD5 1 MPEG2 TrollHD", "A Late Talk Show", 2011, 04, 15)]
        [TestCase("A.Late.Talk.Show.2010.10.11.Johnny.Knoxville.iTouch-MW", "A Late Talk Show", 2010, 10, 11)]
        [TestCase("A Late Talk Show - 2011-04-12 - Gov. Deval Patrick", "A Late Talk Show", 2011, 04, 12)]
        [TestCase("A Late Talk Show - 2011-06-16 - Larry David, \"Bachelorette\" Ashley Hebert, Pitbull with Ne-Yo", "A Late Talk Show", 2011, 6, 16)]
        [TestCase("2020.A.Late.Talk.Show.2012.16.02.PDTV.XviD-C4TV", "2020 A Late Talk Show", 2012, 2, 16)]
        [TestCase("2020.A.Late.Talk.Show.2012.13.02.PDTV.XviD-C4TV", "2020 A Late Talk Show", 2012, 2, 13)]
        [TestCase("2020.A.Late.Talk.Show.2011.12.02.PDTV.XviD-C4TV", "2020 A Late Talk Show", 2011, 12, 2)]
        [TestCase("Series Title - 2013-10-30 - Episode Title (1) [HDTV-720p]", "Series Title", 2013, 10, 30)]
        [TestCase("A.Late.Talk.Show.140722.720p.HDTV.x264-YesTV", "A Late Talk Show", 2014, 07, 22)]
        [TestCase("A_Late_Talk_Show_140722_720p_HDTV_x264-YesTV", "A Late Talk Show", 2014, 07, 22)]

        // [TestCase("Corrie.07.01.15", "Corrie", 2015, 1, 7)]
        [TestCase("The Show Series 2015 02 09 WEBRIP s01e13", "The Show Series", 2015, 2, 9)]
        [TestCase("Tree_Series_2018_06_22_Seth_Meyers_720p_HEVC_x265-MeGusta", "Tree Series", 2018, 6, 22)]
        [TestCase("20161024- Exotic Payback.21x41_720.mkv", "", 2016, 10, 24)]
        [TestCase("Series and Title 20201013 Ep7432 [720p WebRip (x264)] [SUBS]", "Series and Title", 2020, 10, 13)]
        [TestCase("[Site] 19-07-2023 - Loli - Beautiful 2160p {RlsGroup}", "Site", 2023, 07, 19)]

        // [TestCase("", "", 0, 0, 0)]
        public void should_parse_daily_episode(string postTitle, string title, int year, int month, int day)
        {
            var result = Parser.Parser.ParseTitle(postTitle);
            var airDate = new DateTime(year, month, day);
            result.Should().NotBeNull();
            result.SeriesTitle.Should().Be(title);
            result.AirDate.Should().Be(airDate.ToString(Episode.AIR_DATE_FORMAT));
        }

        [TestCase("A Late Talk Show {year} {month} {day} Emma Roberts HDTV XviD BFF")]
        [TestCase("A Late Talk Show {year} {month} {day} 1080i HDTV DD5 1 MPEG2 TrollHD")]
        [TestCase("A.Late.Talk.Show.{year}.{month}.{day}.Johnny.Knoxville.iTouch-MW")]
        [TestCase("A Late Talk Show - {year}-{month}-{day} - Gov. Deval Patrick")]
        [TestCase("{year}.{month}.{day} - A Late Talk Show  - HD TV.mkv")]
        [TestCase("A Late Talk Show - {year}-{month}-{day} - Larry David, \"Bachelorette\" Ashley Hebert, Pitbull with Ne-Yo")]
        [TestCase("2020.NZ.{year}.{month}.{day}.PDTV.XviD-C4TV")]
        public void should_not_accept_ancient_daily_series(string title)
        {
            var yearTooLow = title.Expand(new { year = 1950, month = 10, day = 14 });
            Parser.Parser.ParseTitle(yearTooLow).Should().BeNull();
        }

        [TestCase("A Late Talk Show {year} {month} {day} Emma Roberts HDTV XviD BFF")]
        [TestCase("A Late Talk Show {year} {month} {day} 1080i HDTV DD5 1 MPEG2 TrollHD")]
        [TestCase("A.Late.Talk.Show.{year}.{month}.{day}.Johnny.Knoxville.iTouch-MW")]
        [TestCase("A Late Talk Show  - {year}-{month}-{day} - Gov. Deval Patrick")]
        [TestCase("{year}.{month}.{day} - A Late Talk Show  - HD TV.mkv")]
        [TestCase("A Late Talk Show - {year}-{month}-{day} - Larry David, \"Bachelorette\" Ashley Hebert, Pitbull with Ne-Yo")]
        [TestCase("2020.NZ.{year}.{month}.{day}.PDTV.XviD-C4TV")]
        public void should_not_accept_future_dates(string title)
        {
            var twoDaysFromNow = DateTime.Now.AddDays(2);

            var validDate = title.Expand(new { year = twoDaysFromNow.Year, month = twoDaysFromNow.Month.ToString("00"), day = twoDaysFromNow.Day.ToString("00") });

            Parser.Parser.ParseTitle(validDate).Should().BeNull();
        }

        [Test]
        public void should_fail_if_episode_is_far_in_future()
        {
            var title = string.Format("{0:yyyy.MM.dd} - A Talk Show - HD TV.mkv", DateTime.Now.AddDays(2));

            Parser.Parser.ParseTitle(title).Should().BeNull();
        }
    }
}
