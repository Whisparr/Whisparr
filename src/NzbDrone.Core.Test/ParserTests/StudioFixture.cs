using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.ParserTests
{
    [TestFixture]
    public class StudioFixture : CoreTest
    {
        [TestCase("Brazzers Exxtra", "Brazzers Exxtra")]
        [TestCase("Brazzers Exxtra", "BrazzersExxtra")]
        [TestCase("Hot and Mean", "Hot And Mean")]
        [TestCase("Hot and Mean", "HotAndMean")]
        [TestCase("Monsters of Cock", "MonstersOfCock")]
        [TestCase("In the VIP", "InTheVIP")]
        public void should_match_studio_names(string stashDB, string external)
        {
            // The Clean Title is used to match the record within the DB
            // Test that the sudio name can be found for an external source:
            // FileName
            // Indexer
            stashDB.CleanStudioTitle().Should().Be(external.CleanStudioTitle());
        }
    }
}
