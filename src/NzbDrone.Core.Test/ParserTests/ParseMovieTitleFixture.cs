using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.ParserTests
{
    [TestFixture]
    public class ParseMovieTitleFixture : CoreTest
    {
        [TestCase("Studio - Performer Name - Some Title (10.01.2024)", true)]
        [TestCase("Reflective Desire - 2022-12-01 - Chuck Faerie Vespa - Pussyfootin' - WEBDL-2160p h264", true)]
        [TestCase("MollyRedWolf - 2022-04-14 - MollyRedWolf - Komi-San.I want you - WEBDL-720p x265", true)]
        [TestCase("Studio.19.07.01.Title..480p.MP4-XXX", true)]
        [TestCase("Studio.21.04.09.Title.XXX.480p.MP4-XXX", true)]
        [TestCase("Studio.22.10.18.Title.XXX.720p.HEVC.x265.PRT[XvX]", true)]
        [TestCase("Studio - 2017-08-04 - Some Title. [WEBDL-480p]", true)]
        [TestCase("Movie.2009.S01E14.English.HDTV.XviD-LOL", false)] // Movie
        public void should_parse_as_scene(string title, bool expected)
        {
            Parser.Parser.ParseMovieTitle(title).IsScene.Should().Be(expected);
        }

        [TestCase("Reflective Desire - 2022-12-01 - Chuck Faerie Vespa - Pussyfootin' - WEBDL-2160p h264", "Reflective Desire")]
        [TestCase("MollyRedWolf - 2022-04-14 - MollyRedWolf - Komi-San.I want you - WEBDL-720p x265", "MollyRedWolf")]
        [TestCase("Studio.19.07.01.Title..480p.MP4-XXX", "Studio")]
        [TestCase("Studio.21.04.09.Title.XXX.480p.MP4-XXX", "Studio")]
        [TestCase("Studio.22.10.18.Title.XXX.720p.HEVC.x265.PRT[XvX]", "Studio")]
        [TestCase("Studio - 2017-08-04 - Some Title. [WEBDL-480p]", "Studio")]
        [TestCase("Studio - Performer Name - Some Title (10.01.2024)", "Studio")]
        public void should_correctly_parse_studio_names(string title, string result)
        {
            Parser.Parser.ParseMovieTitle(title).StudioTitle.Should().Be(result);
        }

        [TestCase("Reflective Desire - 2022-12-01 - Chuck Faerie Vespa - Pussyfootin' - WEBDL-2160p h264", "2022-12-01")]
        [TestCase("MollyRedWolf - 2022-04-14 - MollyRedWolf - Komi-San.I want you - WEBDL-720p x265", "2022-04-14")]
        [TestCase("Studio.19.07.01.Title..480p.MP4-XXX", "2019-07-01")]
        [TestCase("Studio.21.04.09.Title.XXX.480p.MP4-XXX", "2021-04-09")]
        [TestCase("Studio.22.10.18.Title.XXX.720p.HEVC.x265.PRT[XvX]", "2022-10-18")]
        [TestCase("Studio - 2017-08-04 - Some Title. [WEBDL-480p]", "2017-08-04")]
        [TestCase("Studio - Performer Name - Some Title (10.01.2024)", "2024-01-10")]
        public void should_correctly_parse_release_date(string title, string result)
        {
            Parser.Parser.ParseMovieTitle(title).ReleaseDate.Should().Be(result);
        }

        [TestCase("Reflective Desire - 2022-12-01 - Chuck Faerie Vespa - Pussyfootin' - WEBDL-2160p h264", "chuck faerie vespa pussyfootin")]
        [TestCase("MollyRedWolf - 2022-04-14 - MollyRedWolf - Komi-San.I want you - WEBDL-720p x265", "mollyredwolf komi san i want you")]
        [TestCase("Studio.22.10.18.This.is.a.XXX.Parody - Scene 1.XXX.720p.HEVC.x265.PRT[XvX]", "this is a xxx parody scene 1")]
        [TestCase("Studio.19.07.01.Title..480p.MP4-XXX", "title")]
        [TestCase("Studio.21.04.09.Title.XXX.480p.MP4-XXX", "title")]
        [TestCase("Studio.22.10.18.Title.XXX.720p.HEVC.x265.PRT[XvX]", "title")]
        [TestCase("Studio - 2017-08-04 - Some Title. [WEBDL-480p]", "some title")]
        [TestCase("Studio - Performer Name - Some Title (10.01.2024)", "performer name some title")]
        public void should_correctly_parse_normalize_release_token(string title, string result)
        {
            var releaseTokens = Parser.Parser.ParseMovieTitle(title).ReleaseTokens;
            var cleanReleaseToken = Parser.Parser.NormalizeEpisodeTitle(releaseTokens);
            cleanReleaseToken.Should().Be(result);
        }
    }
}
