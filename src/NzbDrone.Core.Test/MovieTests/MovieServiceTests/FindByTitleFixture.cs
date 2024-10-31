using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Movies.Performers;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.MovieTests.MovieServiceTests
{
    [TestFixture]
    public class FindByTitleFixture : CoreTest<MovieService>
    {
        private List<Movie> _candidates;

        [SetUp]
        public void Setup()
        {
            var credits = new List<Credit> { new Credit { Character = "Quinn", Performer = new CreditPerformer { Name = "Quinn Waters", Gender = Gender.Female } } };
            var otherCredits = new List<Credit> { new Credit { Character = "Carrie", Performer = new CreditPerformer { Name = "Carrie Sage", Gender = Gender.Female } } };
            var dualCredits = new List<Credit> { new Credit { Character = "Quinn", Performer = new CreditPerformer { Name = "Quinn Waters", Gender = Gender.Female } }, new Credit { Character = "Carrie", Performer = new CreditPerformer { Name = "Carrie Sage", Gender = Gender.Female } } };
            var differentCredits = new List<Credit> { new Credit { Character = "Angie", Performer = new CreditPerformer { Name = "Angela White", Gender = Gender.Female } } };
            var invalidCredits = new List<Credit> { new Credit { Character = "Invalid", Performer = new CreditPerformer { Name = "Invalid Name", Gender = Gender.Female } } };
            var bellaCredits = new List<Credit> { new Credit { Character = "", Performer = new CreditPerformer { Name = "Violet Myers", Gender = Gender.Female } }, new Credit { Character = "", Performer = new CreditPerformer { Name = "Victor Ray", Gender = Gender.Male } } };
            var evilCredits = new List<Credit> { new Credit { Character = "", Performer = new CreditPerformer { Name = "Whitney Wright", Gender = Gender.Female } }, new Credit { Character = "", Performer = new CreditPerformer { Name = "Mick Blue", Gender = Gender.Male } } };

            var studio = new Core.MetadataSource.SkyHook.Resource.StudioResource { Title = "Studio" };
            var evilStudio = new Core.MetadataSource.SkyHook.Resource.StudioResource { Title = "EvilAngel" };

            var scenes = Builder<Movie>.CreateListOfSize(2000)
                                        .TheFirst(1)
                                        .With(x => x.Title = "Invalid")
                                        .With(x => x.MovieMetadata.Value.ReleaseDate = "2020-01-01")
                                        .With(x => x.MovieMetadata.Value.Studio = studio)
                                        .With(x => x.MovieMetadata.Value.Credits = invalidCredits)
                                        .TheNext(1)
                                        .With(x => x.Title = "Title Vol 1 E2")
                                        .With(x => x.MovieMetadata.Value.ReleaseDate = "2020-05-29")
                                        .With(x => x.MovieMetadata.Value.Studio = studio)
                                        .TheNext(1)
                                        .With(x => x.Title = "Title")
                                        .With(x => x.MovieMetadata.Value.ReleaseDate = "2020-04-01")
                                        .With(x => x.MovieMetadata.Value.Credits = credits)
                                        .With(x => x.MovieMetadata.Value.Studio = studio)
                                        .TheNext(1)
                                        .With(x => x.Title = "Episode Title")
                                        .With(x => x.MovieMetadata.Value.ReleaseDate = "2020-04-02")
                                        .With(x => x.MovieMetadata.Value.Credits = credits)
                                        .With(x => x.MovieMetadata.Value.Studio = studio)
                                        .TheNext(1)
                                        .With(x => x.Title = "Episode Title")
                                        .With(x => x.MovieMetadata.Value.ReleaseDate = "2024-06-11")
                                        .With(x => x.MovieMetadata.Value.Credits = credits)
                                        .With(x => x.MovieMetadata.Value.Studio = studio)
                                        .TheNext(1)
                                        .With(x => x.Title = "Milk & Chocolate Before Bed🥛🕟😵‍💫🕦🥛")
                                        .With(x => x.MovieMetadata.Value.ReleaseDate = "2024-07-30")
                                        .With(x => x.MovieMetadata.Value.Credits = credits)
                                        .With(x => x.MovieMetadata.Value.Studio = studio)
                                        .TheNext(1)
                                        .With(x => x.Title = "Title")
                                        .With(x => x.MovieMetadata.Value.ReleaseDate = "2021-01-08")
                                        .With(x => x.MovieMetadata.Value.Credits = credits)
                                        .With(x => x.MovieMetadata.Value.Studio = studio)
                                        .TheNext(1)
                                        .With(x => x.Title = "Other Title")
                                        .With(x => x.MovieMetadata.Value.ReleaseDate = "2021-01-09")
                                        .With(x => x.MovieMetadata.Value.Credits = dualCredits)
                                        .With(x => x.MovieMetadata.Value.Studio = studio)
                                        .TheNext(1)
                                        .With(x => x.Title = "White Winter")
                                        .With(x => x.MovieMetadata.Value.ReleaseDate = "2021-01-09")
                                        .With(x => x.MovieMetadata.Value.Credits = differentCredits)
                                        .With(x => x.MovieMetadata.Value.Studio = studio)
                                        .TheNext(1)
                                        .With(x => x.Title = "Another Title")
                                        .With(x => x.MovieMetadata.Value.ReleaseDate = "2021-01-08")
                                        .With(x => x.MovieMetadata.Value.Credits = otherCredits)
                                        .With(x => x.MovieMetadata.Value.Studio = studio)
                                        .TheNext(1)
                                        .With(x => x.Title = "Title")
                                        .With(x => x.MovieMetadata.Value.ReleaseDate = "2019-05-18")
                                        .With(x => x.MovieMetadata.Value.Credits = credits)
                                        .With(x => x.MovieMetadata.Value.Studio = studio)
                                        .TheNext(1)
                                        .With(x => x.Title = "Title")
                                        .With(x => x.MovieMetadata.Value.ReleaseDate = "2019-05-18")
                                        .With(x => x.MovieMetadata.Value.Credits = dualCredits)
                                        .With(x => x.MovieMetadata.Value.Studio = studio)
                                        .TheNext(1)
                                        .With(x => x.Title = "Other Title")
                                        .With(x => x.MovieMetadata.Value.ReleaseDate = "2019-05-18")
                                        .With(x => x.MovieMetadata.Value.Credits = credits)
                                        .With(x => x.MovieMetadata.Value.Studio = studio)
                                        .TheNext(1)
                                        .With(x => x.Title = "Another Title")
                                        .With(x => x.MovieMetadata.Value.ReleaseDate = "2019-05-18")
                                        .With(x => x.MovieMetadata.Value.Credits = otherCredits)
                                        .With(x => x.MovieMetadata.Value.Studio = studio)
                                        .TheNext(1)
                                        .With(x => x.Title = "Other Title")
                                        .With(x => x.MovieMetadata.Value.ReleaseDate = "2019-05-18")
                                        .With(x => x.MovieMetadata.Value.Credits = credits)
                                        .With(x => x.MovieMetadata.Value.Studio = studio)
                                        .TheNext(1)
                                        .With(x => x.Title = "Episode 200: Violet & Victor")
                                        .With(x => x.MovieMetadata.Value.ReleaseDate = "2024-08-15")
                                        .With(x => x.MovieMetadata.Value.Credits = bellaCredits)
                                        .With(x => x.MovieMetadata.Value.Studio = studio)
                                        .TheNext(1)
                                        .With(x => x.Title = "Cash For Kisses On Valentines Day - S25:E7")
                                        .With(x => x.MovieMetadata.Value.ReleaseDate = "2024-02-07")
                                        .With(x => x.MovieMetadata.Value.Credits = credits)
                                        .With(x => x.MovieMetadata.Value.Studio = studio)
                                        .TheNext(1)
                                        .With(x => x.Title = "Whitney Wright POV Anal & A2M")
                                        .With(x => x.MovieMetadata.Value.ReleaseDate = "2021-02-24")
                                        .With(x => x.MovieMetadata.Value.Studio = evilStudio)
                                        .With(x => x.MovieMetadata.Value.Credits = evilCredits)
                                        .TheNext(1)
                                        .With(x => x.Title = "BTS Whitney Wright POV Anal & A2M")
                                        .With(x => x.MovieMetadata.Value.ReleaseDate = "2021-02-24")
                                        .With(x => x.MovieMetadata.Value.Studio = evilStudio)
                                        .With(x => x.MovieMetadata.Value.Credits = evilCredits)
                                        .TheRest()
                                        .With(x => x.Title = "Title For the Rest")
                                        .With(x => x.MovieMetadata.Value.ReleaseDate = "2024-06-12")
                                        .With(x => x.MovieMetadata.Value.Credits = credits)
                                        .With(x => x.MovieMetadata.Value.Studio = studio)
                                        .Build()
                                        .ToList();

            Mocker.GetMock<IMovieRepository>()
                .Setup(s => s.GetByStudioForeignId(It.Is<string>(s => s.Equals("Studio"))))
                .Returns(scenes.Where(s => s.MovieMetadata.Value.Studio.Title.Equals("Studio")).ToList());

            Mocker.GetMock<IMovieRepository>()
                .Setup(s => s.GetByStudioForeignId(It.Is<string>(s => s.Equals("EvilAngel"))))
                .Returns(scenes.Where(s => s.MovieMetadata.Value.Studio.Title.Equals("EvilAngel")).ToList());

            Mocker.GetMock<IMovieRepository>()
                .Setup(s => s.FindByStudioAndDate(It.Is<string>(s => s.Equals("Studio")), It.Is<string>(d => d.Equals("2021-01-08"))))
                .Returns(scenes.Where(s => s.MovieMetadata.Value.ReleaseDate.Equals("2021-01-08")).Append(scenes.First()).ToList());

            Mocker.GetMock<IMovieRepository>()
                .Setup(s => s.FindByStudioAndDate(It.Is<string>(s => s.Equals("Studio")), It.Is<string>(d => d.Equals("2021-01-09"))))
                .Returns(scenes.Where(s => s.MovieMetadata.Value.ReleaseDate.Equals("2021-01-09")).Append(scenes.First()).ToList());

            Mocker.GetMock<IMovieRepository>()
                .Setup(s => s.FindByStudioAndDate(It.Is<string>(s => s.Equals("Studio")), It.Is<string>(d => d.Equals("2020-05-29"))))
                .Returns(scenes.Where(s => s.MovieMetadata.Value.ReleaseDate.Equals("2020-05-29")).Append(scenes.First()).ToList());

            Mocker.GetMock<IMovieRepository>()
                .Setup(s => s.FindByStudioAndDate(It.Is<string>(s => s.Equals("Studio")), It.Is<string>(d => d.Equals("2020-04-01"))))
                .Returns(scenes.Where(s => s.MovieMetadata.Value.ReleaseDate.Equals("2020-04-01")).Append(scenes.First()).ToList());

            Mocker.GetMock<IMovieRepository>()
                .Setup(s => s.FindByStudioAndDate(It.Is<string>(s => s.Equals("Studio")), It.Is<string>(d => d.Equals("2024-06-11"))))
                .Returns(scenes.Where(s => s.MovieMetadata.Value.ReleaseDate.Equals("2024-06-11")).Append(scenes.First()).ToList());

            Mocker.GetMock<IMovieRepository>()
                .Setup(s => s.FindByStudioAndDate(It.Is<string>(s => s.Equals("Studio")), It.Is<string>(d => d.Equals("2024-07-30"))))
                .Returns(scenes.Where(s => s.MovieMetadata.Value.ReleaseDate.Equals("2024-07-30")).Append(scenes.First()).ToList());

            Mocker.GetMock<IMovieRepository>()
                .Setup(s => s.FindByStudioAndDate(It.Is<string>(s => s.Equals("Studio")), It.Is<string>(d => d.Equals("2019-05-18"))))
                .Returns(scenes.Where(s => s.MovieMetadata.Value.ReleaseDate.Equals("2019-05-18")).Append(scenes.First()).ToList());

            Mocker.GetMock<IMovieRepository>()
                .Setup(s => s.FindByStudioAndDate(It.Is<string>(s => s.Equals("Bellesa House")), It.Is<string>(d => d.Equals("2024-08-15"))))
                .Returns(scenes.Where(s => s.MovieMetadata.Value.ReleaseDate.Equals("2024-08-15")).Append(scenes.First()).ToList());

            Mocker.GetMock<IMovieRepository>()
                .Setup(s => s.FindByStudioAndDate(It.Is<string>(s => s.Equals("Step Siblings Caught")), It.Is<string>(d => d.Equals("2024-02-07"))))
                .Returns(scenes.Where(s => s.MovieMetadata.Value.ReleaseDate.Equals("2024-02-07")).Append(scenes.First()).ToList());

            Mocker.GetMock<IMovieRepository>()
                .Setup(s => s.FindByStudioAndDate(It.Is<string>(s => s.Equals("EvilAngel")), It.Is<string>(d => d.Equals("2021-02-24"))))
                .Returns(scenes.Where(s => s.MovieMetadata.Value.ReleaseDate.Equals("2021-02-24")).Append(scenes.First()).ToList());

            _candidates = Builder<Movie>.CreateListOfSize(3)
                                        .TheFirst(1)
                                        .With(x => x.MovieMetadata.Value.CleanTitle = "batman")
                                        .With(x => x.Year = 2000)
                                        .TheNext(1)
                                        .With(x => x.MovieMetadata.Value.CleanTitle = "batman")
                                        .With(x => x.Year = 1999)
                                        .TheRest()
                                        .With(x => x.MovieMetadata.Value.CleanTitle = "darkknight")
                                        .With(x => x.Year = 2008)
                                        .Build()
                                        .ToList();
        }

        [Test]
        public void should_find_by_title_year()
        {
            var movie = Subject.FindByTitle(new List<string> { "batman" }, 2000, new List<string>(), _candidates);

            movie.Should().NotBeNull();
            movie.Year.Should().Be(2000);
        }

        [TestCase("Studio 2020-05-29 Title Vol 1 E2", 2)]
        [TestCase("Studio 2020-05-29 Title Vol 1 E2_1", 2)]
        [TestCase("[Studio] Quinn Waters (Title / 08.01.2021) [2021 г., Big Tits, Blowjob, Brunette, Chubby, Curvy, Cowgirl, Reverse Cowgirl, Cumshots, Facials, Long Hair, Doggy Style, Hardcore, Missionary, PAWG, POV, Trimmed Pussy, Tattoo, Czech, VR, 8K, 3840p] [Oculus Rift / Vive]", 7)]
        [TestCase("Studio.21.01.08.Title", 7)]
        [TestCase("Studio.21.01.08.Quinn Waters", 7)]
        [TestCase("Studio.21.01.08.Quinn", 7)]
        [TestCase("Studio.21.01.09.Quinn and Carrie", 8)]
        [TestCase("Studio.21.01.09.Quinn & Carrie", 8)]
        [TestCase("Studio.21.01.09.Quinn & Carrie - Other Title", 8)]
        [TestCase("Studio.21.01.08.Carrie", 10)]
        [TestCase("Studio.21.01.08.Carrie Sage", 10)]
        [TestCase("Studio - 2024-07-30 - Milk & Chocolate Before Bed", 6)]
        [TestCase("Bellesa House 2024-08-15 Episode 200 Violet And Victor", 16)]
        [TestCase("Bellesa House 2024-08-15 Episode 200 Violet & Victor", 16)]
        [TestCase("Step Siblings Caught 2024-02-07 Cash For Kisses On Valentines Day - S25E7", 17)]
        [TestCase("EvilAngel - 2021-02-24 - BTS Whitney Wright POV Anal & A2M", 18)]
        [TestCase("EvilAngel - 2021-02-24 - Whitney Wright POV Anal & A2M", 19)]
        [TestCase("EvilAngel.E1224.Whitney Wright POV Anal & A2M", 19)] // Episode (Search all for the studio)
        [TestCase("EvilAngel - 2021-02-24 - Whitney Wright", null)]  // Possible Duplicate so no match
        public void should_find_by_studio_and_release_date(string title, int? id)
        {
            var parsedMovieInfo = Parser.Parser.ParseMovieTitle(title);

            if (parsedMovieInfo.IsScene)
            {
                var movie = Subject.FindByStudioAndReleaseDate(parsedMovieInfo.StudioTitle, parsedMovieInfo.ReleaseDate, parsedMovieInfo.ReleaseTokens);

                if (id != null)
                {
                    movie.Should().NotBeNull();
                    movie.Id.Should().Be(id);
                }
                else
                {
                    // Should not match as duplicate
                    movie.Should().BeNull();
                }
            }
        }
    }
}
