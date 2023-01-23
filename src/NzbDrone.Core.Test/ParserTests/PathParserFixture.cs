using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.ParserTests
{
    [TestFixture]
    public class PathParserFixture : CoreTest
    {
        [TestCase(@"/Test/Site/2023/Site - 2023-01-23.m4v", "2023-01-23")]
        [TestCase(@"C:\Test\Site\Site - 2023-01-23.m4v", "2023-01-23")]
        [TestCase(@"C:\Test\Site\2023\Site - 2023-01-23.m4v", "2023-01-23")]
        [TestCase(@"C:\Test\Site\2023\Site - 2023-01-23 - Scene Title.m4v", "2023-01-23")]
        public void should_parse_from_path(string path, string airDate)
        {
            var result = Parser.Parser.ParsePath(path.AsOsAgnostic());

            result.AirDate.Should().Be(airDate);
            result.FullSeason.Should().BeFalse();

            ExceptionVerification.IgnoreWarns();
        }
    }
}
