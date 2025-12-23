using System.Collections.Generic;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Movies.Credits;
using NzbDrone.Core.Movies.Performers;
using NzbDrone.Core.Movies.Studios;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.OrganizerTests
{
    [TestFixture]

    public class GetMovieFolderFixture : CoreTest<FileNameBuilder>
    {
        private NamingConfig _namingConfig;
        private Movie _scene;

        [SetUp]
        public void Setup()
        {
            _namingConfig = NamingConfig.Default;

            var studio = new Studio { Title = "Studio's Name", Network = "Network's Name", ForeignId = "StashId" };

            var credits = new List<Credit>
            {
                new Credit { Character = "Rissa", Performer = new CreditPerformer { Name = "Rissa May", Gender = Gender.Female } },
                new Credit { Character = null, Performer = new CreditPerformer { Name = "Maddy O'Reilly", Gender = Gender.Female } },
                new Credit { Character = "Chuck", Performer = new CreditPerformer { Name = "Charles Dera", Gender = Gender.Male } },
                new Credit { Character = "Reagan", Performer = new CreditPerformer { Name = "Reagan Foxx", Gender = Gender.Female } },
                new Credit { Character = "Axel", Performer = new CreditPerformer { Name = "Axel Haze", Gender = Gender.Male } },
                new Credit { Character = null, Performer = new CreditPerformer { Name = "Manuel Ferrara", Gender = Gender.Male } }
            };

            _scene = Builder<Movie>
                    .CreateNew()
                    .With(s => s.Title = "The Last Train Home")
                    .With(x => x.ForeignId = "019abb52-0557-7c5f-83df-94b828851fd1")
                    .With(x => x.MovieMetadata.Value.ForeignId = "019abb52-0557-7c5f-83df-94b828851fd1")
                    .With(x => x.MovieMetadata.Value.StashId = "019abb52-0557-7c5f-83df-94b828851fd1")
                    .With(x => x.MovieMetadata.Value.ReleaseDate = "2025-11-25")
                    .With(x => x.MovieMetadata.Value.Credits = credits)
                    .With(x => x.MovieMetadata.Value.StudioForeignId = studio.ForeignId)
                    .With(x => x.MovieMetadata.Value.StudioTitle = studio.Title)
                    .With(x => x.MovieMetadata.Value.ItemType = ItemType.Scene)
                    .Build();

            Mocker.GetMock<INamingConfigService>()
                  .Setup(c => c.GetConfig()).Returns(_namingConfig);

            Mocker.GetMock<IStudioService>()
                .Setup(c => c.FindByForeignId(It.Is<string>(s => s.Equals("StashId"))))
                .Returns(studio);
        }

        [TestCase("Arrival", 2016, "{Movie Title} ({Release Year})", "Arrival (2016)")]
        [TestCase("The Big Short", 2015, "{Movie TitleThe} ({Release Year})", "Big Short, The (2015)")]
        [TestCase("The Big Short", 2015, "{Movie Title} ({Release Year})", "The Big Short (2015)")]
        public void should_use_movieFolderFormat_to_build_folder_name(string movieTitle, int year, string format, string expected)
        {
            _namingConfig.MovieFolderFormat = format;

            var movie = new Movie { Title = movieTitle, Year = year };

            Subject.GetMovieFolder(movie).Should().Be(expected);
        }

        [TestCase("scenes-{Studio Network}-{Studio Title}-{Release Date} - {Scene CleanTitle} {[StashId]}", "scenes-Network's Name-Studio's Name-2025-11-25 - The Last Train Home [019abb52-0557-7c5f-83df-94b828851fd1]")]
        [TestCase("scenes-{Studio CleanNetwork}-{Studio CleanTitle}-{Release Date} - {Scene CleanTitle} {[StashId]}", "scenes-Networks Name-Studios Name-2025-11-25 - The Last Train Home [019abb52-0557-7c5f-83df-94b828851fd1]")]
        public void should_use_sceneFolderFormat_to_build_folder_name(string format, string expected)
        {
            _namingConfig.SceneFolderFormat = format;

            Subject.GetMovieFolder(_scene).Should().Be(expected);
        }
    }
}
