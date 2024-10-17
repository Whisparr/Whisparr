using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.ParserTests
{
    [TestFixture]
    public class ParseMovieTitleFixture : CoreTest
    {
        [TestCase("Studio 2020-05-29 Title Vol 1 E2", true)]
        [TestCase("LoveHerBoobs 24 06 11 Peachy Alice Successful Provocation XXX 720p AV1 XLeech.mkv", true)]
        [TestCase("Studio.E1224.Title.XXX.720p.HEVC.x265.PRT[XvX]", true)]
        [TestCase("[Studio] Performer Name (Title / 08.01.2021) [2021 г., Big Tits, Blowjob, Brunette, Chubby, Curvy, Cowgirl, Reverse Cowgirl, Cumshots, Facials, Long Hair, Doggy Style, Hardcore, Missionary, PAWG, POV, Trimmed Pussy, Tattoo, Czech, VR, 8K, 3840p] [Oculus Rift / Vive]", true)]
        [TestCase("[Studio.com] Performer & Performer - Studio Title (18.05.2019) [2019 г., Anal, IR, Rim Job, Ass To Mouth, Big Cocks, Black, Blowjobs, Brunettes, Deep Throat, Facial, Gaping, Natural, Teen, 1080p]", true)]
        [TestCase("[random.com][Studio] Performer name - Title(01.04.2020) rq.mp4", true)]
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

        [TestCase("Studio 2020-05-29 Title Vol 1 E2", "Studio")]
        [TestCase("LoveHerBoobs 24 06 11 Peachy Alice Successful Provocation XXX 720p AV1 XLeech.mkv", "LoveHerBoobs")]
        [TestCase("Studio.E1224.Title.XXX.720p.HEVC.x265.PRT[XvX]", "Studio")]
        [TestCase("[Studio] Performer Name (Title / 08.01.2021) [2021 г., Big Tits, Blowjob, Brunette, Chubby, Curvy, Cowgirl, Reverse Cowgirl, Cumshots, Facials, Long Hair, Doggy Style, Hardcore, Missionary, PAWG, POV, Trimmed Pussy, Tattoo, Czech, VR, 8K, 3840p] [Oculus Rift / Vive]", "Studio")]
        [TestCase("[Studio.com] Performer & Performer - Studio Title (18.05.2019) [2019 г., Anal, IR, Rim Job, Ass To Mouth, Big Cocks, Black, Blowjobs, Brunettes, Deep Throat, Facial, Gaping, Natural, Teen, 1080p]", "Studio")]
        [TestCase("[random.com][Studio] Performer name - Title(01.04.2020) rq.mp4", "Studio")]
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

        [TestCase("Studio 2020-05-29 Title Vol 1 E2", "2020-05-29")]
        [TestCase("LoveHerBoobs 24 06 11 Peachy Alice Successful Provocation XXX 720p AV1 XLeech.mkv", "2024-06-11")]
        [TestCase("[Studio] Performer Name (Title / 08.01.2021) [2021 г., Big Tits, Blowjob, Brunette, Chubby, Curvy, Cowgirl, Reverse Cowgirl, Cumshots, Facials, Long Hair, Doggy Style, Hardcore, Missionary, PAWG, POV, Trimmed Pussy, Tattoo, Czech, VR, 8K, 3840p] [Oculus Rift / Vive]", "2021-01-08")]
        [TestCase("[Studio.com] Performer & Performer - Studio Title (18.05.2019) [2019 г., Anal, IR, Rim Job, Ass To Mouth, Big Cocks, Black, Blowjobs, Brunettes, Deep Throat, Facial, Gaping, Natural, Teen, 1080p]", "2019-05-18")]
        [TestCase("[random.com][Studio] Performer name - Title(01.04.2020) rq.mp4", "2020-04-01")]
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

        [TestCase("Studio 2020-05-29 Title Vol 1 E2", "title vol 1 e2")]
        [TestCase("LoveHerBoobs 24 06 11 Peachy Alice Successful Provocation XXX 720p AV1 XLeech.mkv", "peachy alice successful provocation")]
        [TestCase("Studio.E1224.Title.XXX.720p.HEVC.x265.PRT[XvX]", "title")]
        [TestCase("[Studio] Performer Name (Title / 08.01.2021) [2021 г., Big Tits, Blowjob, Brunette, Chubby, Curvy, Cowgirl, Reverse Cowgirl, Cumshots, Facials, Long Hair, Doggy Style, Hardcore, Missionary, PAWG, POV, Trimmed Pussy, Tattoo, Czech, VR, 8K, 3840p] [Oculus Rift / Vive]", "performer name title")]
        [TestCase("[Studio.com] Performer & Performer - Studio Title (18.05.2019) [2019 г., Anal, IR, Rim Job, Ass To Mouth, Big Cocks, Black, Blowjobs, Brunettes, Deep Throat, Facial, Gaping, Natural, Teen, 1080p]", "performer and performer studio title")]
        [TestCase("[random.com][Studio] Performer name - Title(01.04.2020) rq.mp4", "performer name title")]
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
