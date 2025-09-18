using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Parser;

namespace NzbDrone.Core.Test.ParserTests
{
    [TestFixture]
    public class ExternalIdParserFixture
    {
        [TestCase("[SKMJ-649] Amateur female college students found at hot springs all over Japan. Would you like to try entering the men's bath with just a towel on? 15 ordinary tourists who were traveling normally until just now. All of them have raw creampie sin this luxurious 3-hour special.", "SKMJ-649")]
        [TestCase("[ABC-123] Test Title", "ABC-123")]
        [TestCase("(XYZ-456) Another Test", "XYZ-456")]
        [TestCase("{DEF-789} Curly Brackets", "DEF-789")]
        [TestCase("GHI-012 Starting Title", "GHI-012")]
        [TestCase("[JKL_345] Underscore Separator", "JKL_345")]
        [TestCase("[MNO.678] Dot Separator", "MNO.678")]
        [TestCase("[ABCD 123] Space Separator", "ABCD 123")]
        [TestCase("WXYZ-999 Title Without Brackets", "WXYZ-999")]
        [TestCase("No External ID Here", null)]
        [TestCase("", null)]
        [TestCase(null, null)]
        [TestCase("Random [Text] Without ID Pattern", null)]
        [TestCase("[123] Number Only", null)]
        [TestCase("[ABC] Letters Only", null)]
        public void should_extract_external_id(string releaseTitle, string expectedExternalId)
        {
            var result = ExternalIdParser.ExtractExternalId(releaseTitle);
            result.Should().Be(expectedExternalId);
        }

        [TestCase("[SKMJ-649] Test Title", "SKMJ-649", true)]
        [TestCase("[SKMJ-649] Test Title", "skmj-649", true)]
        [TestCase("[SKMJ-649] Test Title", "ABC-123", false)]
        [TestCase("No ID Here", "SKMJ-649", false)]
        public void should_check_contains_external_id(string releaseTitle, string externalId, bool expectedResult)
        {
            var result = ExternalIdParser.ContainsExternalId(releaseTitle, externalId);
            result.Should().Be(expectedResult);
        }
    }
}
