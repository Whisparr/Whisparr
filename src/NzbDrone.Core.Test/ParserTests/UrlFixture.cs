using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.ParserTests
{
    [TestFixture]
    public class UrlFixture : CoreTest
    {
        [TestCase("Series.2009.S01E14.English.HDTV.XviD-LOL[www.abb.com]", "LOL")]
        [TestCase("Series 2009 S01E14 English HDTV XviD LOL[www.academy.org]", null)]
        [TestCase("Series Now S05 EXTRAS DVDRip XviD RUNNER[www.aetna.net]", null)]
        [TestCase("Series.Title.S01.EXTRAS.DVDRip.XviD-RUNNER[www.alfaromeo.io]", "RUNNER")]
        [TestCase("2020.Series.2011.12.02.PDTV.XviD-C4TV[rarbg.to]", "C4TV")]
        [TestCase("Series.Title.S01E14.English.HDTV.XviD-LOL[www.abbott.gov]", "LOL")]
        [TestCase("Series 2020 S01E14 English HDTV XviD LOL[www.actor.org]", null)]
        [TestCase("Series Live S05 EXTRAS DVDRip XviD RUNNER[www.agency.net]", null)]
        [TestCase("Series.Title.S02.EXTRAS.DVDRip.XviD-RUNNER[www.airbus.io]", "RUNNER")]
        [TestCase("2021.Series.2012.12.02.PDTV.XviD-C4TV[rarbg.to]", "C4TV")]
        [TestCase("Series.2020.S01E14.English.HDTV.XviD-LOL[www.afl.com]", "LOL")]
        [TestCase("Series 2021 S01E14 English HDTV XviD LOL[www.adult.org]", null)]
        [TestCase("Series Future S05 EXTRAS DVDRip XviD RUNNER[www.allstate.net]", null)]
        [TestCase("Series.Title.S03.EXTRAS.DVDRip.XviD-RUNNER[www.ally.io]", "RUNNER")]
        [TestCase("2022.Series.2013.12.02.PDTV.XviD-C4TV[rarbg.to]", "C4TV")]

        public void should_not_parse_url_in_group(string title, string expected)
        {
            Parser.Parser.ParseReleaseGroup(title).Should().Be(expected);
        }
    }
}
