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
            var othercredits = new List<Credit> { new Credit { Character = "Carrie", Performer = new CreditPerformer { Name = "Carrie Sage", Gender = Gender.Female } } };
            var dualcredits = new List<Credit> { new Credit { Character = "Quinn", Performer = new CreditPerformer { Name = "Quinn WatersName", Gender = Gender.Female } }, new Credit { Character = "Carrie", Performer = new CreditPerformer { Name = "Carrie Sage", Gender = Gender.Female } } };
            var scenes = Builder<Movie>.CreateListOfSize(2000)
                                        .TheFirst(1)
                                        .With(x => x.Title = "Title Vol 1 E2")
                                        .With(x => x.MovieMetadata.Value.ReleaseDate = "2020-05-29")
                                        .TheNext(1)
                                        .With(x => x.Title = "Title")
                                        .With(x => x.MovieMetadata.Value.ReleaseDate = "2020-04-01")
                                        .With(x => x.MovieMetadata.Value.Credits = credits)
                                        .TheNext(1)
                                        .With(x => x.Title = "Episode Title")
                                        .With(x => x.MovieMetadata.Value.ReleaseDate = "2020-04-02")
                                        .With(x => x.MovieMetadata.Value.Credits = credits)
                                        .TheNext(1)
                                        .With(x => x.Title = "Episode Title")
                                        .With(x => x.MovieMetadata.Value.ReleaseDate = "2024-06-11")
                                        .With(x => x.MovieMetadata.Value.Credits = credits)
                                        .TheNext(1)
                                        .With(x => x.Title = "Milk & Chocolate Before Bed🥛🕟😵‍💫🕦🥛")
                                        .With(x => x.MovieMetadata.Value.ReleaseDate = "2024-07-30")
                                        .With(x => x.MovieMetadata.Value.Credits = credits)
                                        .TheNext(1)
                                        .With(x => x.Title = "Title")
                                        .With(x => x.MovieMetadata.Value.ReleaseDate = "2021-01-08")
                                        .With(x => x.MovieMetadata.Value.Credits = credits)
                                        .TheNext(1)
                                        .With(x => x.Title = "Other Title")
                                        .With(x => x.MovieMetadata.Value.ReleaseDate = "2021-01-09")
                                        .With(x => x.MovieMetadata.Value.Credits = dualcredits)
                                        .TheNext(1)
                                        .With(x => x.Title = "Another Title")
                                        .With(x => x.MovieMetadata.Value.ReleaseDate = "2021-01-08")
                                        .With(x => x.MovieMetadata.Value.Credits = othercredits)
                                        .TheNext(1)
                                        .With(x => x.Title = "Title")
                                        .With(x => x.MovieMetadata.Value.ReleaseDate = "2019-05-18")
                                        .With(x => x.MovieMetadata.Value.Credits = credits)
                                        .TheNext(1)
                                        .With(x => x.Title = "Title")
                                        .With(x => x.MovieMetadata.Value.ReleaseDate = "2019-05-18")
                                        .With(x => x.MovieMetadata.Value.Credits = dualcredits)
                                        .TheNext(1)
                                        .With(x => x.Title = "Other Title")
                                        .With(x => x.MovieMetadata.Value.ReleaseDate = "2019-05-18")
                                        .With(x => x.MovieMetadata.Value.Credits = credits)
                                        .TheNext(1)
                                        .With(x => x.Title = "Another Title")
                                        .With(x => x.MovieMetadata.Value.ReleaseDate = "2019-05-18")
                                        .With(x => x.MovieMetadata.Value.Credits = othercredits)
                                        .TheNext(1)
                                        .With(x => x.Title = "Other Title")
                                        .With(x => x.MovieMetadata.Value.ReleaseDate = "2019-05-18")
                                        .With(x => x.MovieMetadata.Value.Credits = credits)
                                        .TheRest()
                                        .With(x => x.Title = "Title")
                                        .With(x => x.MovieMetadata.Value.ReleaseDate = "2024-06-12")
                                        .With(x => x.MovieMetadata.Value.Credits = credits)
                                        .Build()
                                        .ToList();

            Mocker.GetMock<IMovieRepository>()
                .Setup(s => s.GetByStudioForeignId(It.Is<string>(s => s.Equals("Studio"))))
                .Returns(scenes);

            Mocker.GetMock<IMovieRepository>()
                .Setup(s => s.FindByStudioAndDate(It.Is<string>(s => s.Equals("Studio")), It.Is<string>(d => d.Equals("2021-01-08"))))
                .Returns(scenes.Where(s => s.MovieMetadata.Value.ReleaseDate.Equals("2021-01-08")).ToList());

            Mocker.GetMock<IMovieRepository>()
                .Setup(s => s.FindByStudioAndDate(It.Is<string>(s => s.Equals("Studio")), It.Is<string>(d => d.Equals("2021-01-09"))))
                .Returns(scenes.Where(s => s.MovieMetadata.Value.ReleaseDate.Equals("2021-01-09")).ToList());

            Mocker.GetMock<IMovieRepository>()
                .Setup(s => s.FindByStudioAndDate(It.Is<string>(s => s.Equals("Studio")), It.Is<string>(d => d.Equals("2020-05-29"))))
                .Returns(scenes.Where(s => s.MovieMetadata.Value.ReleaseDate.Equals("2020-05-29")).ToList());

            Mocker.GetMock<IMovieRepository>()
                .Setup(s => s.FindByStudioAndDate(It.Is<string>(s => s.Equals("Studio")), It.Is<string>(d => d.Equals("2020-04-01"))))
                .Returns(scenes.Where(s => s.MovieMetadata.Value.ReleaseDate.Equals("2020-04-01")).ToList());

            Mocker.GetMock<IMovieRepository>()
                .Setup(s => s.FindByStudioAndDate(It.Is<string>(s => s.Equals("Studio")), It.Is<string>(d => d.Equals("2024-06-11"))))
                .Returns(scenes.Where(s => s.MovieMetadata.Value.ReleaseDate.Equals("2024-06-11")).ToList());

            Mocker.GetMock<IMovieRepository>()
                .Setup(s => s.FindByStudioAndDate(It.Is<string>(s => s.Equals("Studio")), It.Is<string>(d => d.Equals("2024-07-30"))))
                .Returns(scenes.Where(s => s.MovieMetadata.Value.ReleaseDate.Equals("2024-07-30")).ToList());

            Mocker.GetMock<IMovieRepository>()
                .Setup(s => s.FindByStudioAndDate(It.Is<string>(s => s.Equals("Studio")), It.Is<string>(d => d.Equals("2019-05-18"))))
                .Returns(scenes.Where(s => s.MovieMetadata.Value.ReleaseDate.Equals("2019-05-18")).ToList());

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

        // [TestCase("Studio.E1224.Episode Title.XXX.720p.HEVC.x265.PRT[XvX]", 3)]
        // [TestCase("[Studio.com] Performer Name & Other Performer - Title (18.05.2019)", 7)]

        [TestCase("Studio 2020-05-29 Title Vol 1 E2", 1)]
        [TestCase("[Studio] Quinn Waters (Title / 08.01.2021) [2021 г., Big Tits, Blowjob, Brunette, Chubby, Curvy, Cowgirl, Reverse Cowgirl, Cumshots, Facials, Long Hair, Doggy Style, Hardcore, Missionary, PAWG, POV, Trimmed Pussy, Tattoo, Czech, VR, 8K, 3840p] [Oculus Rift / Vive]", 6)]
        [TestCase("Studio.21.01.08.Title", 6)]
        [TestCase("Studio.21.01.08.Quinn Waters", 6)]
        [TestCase("Studio.21.01.08.Quinn", 6)]
        [TestCase("Studio.21.01.09.Quinn & Carrie", 7)]
        [TestCase("Studio.21.01.08.Carrie", 8)]
        [TestCase("Studio.21.01.08.Carrie Sage", 8)]
        [TestCase("Studio - 2024-07-30 - Milk & Chocolate Before Bed", 5)]
        public void should_find_by_studio_and_release_date(string title, int id)
        {
            var parsedMovieInfo = Parser.Parser.ParseMovieTitle(title);

            if (parsedMovieInfo.IsScene)
            {
                var movie = Subject.FindByStudioAndReleaseDate(parsedMovieInfo.StudioTitle, parsedMovieInfo.ReleaseDate, parsedMovieInfo.ReleaseTokens);

                movie.Should().NotBeNull();
                movie.Id.Should().Be(id);
            }
        }
    }
}
