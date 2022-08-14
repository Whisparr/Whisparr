using System;
using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Exceptions;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Movies.Commands;
using NzbDrone.Core.Movies.Credits;
using NzbDrone.Core.RootFolders;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.MovieTests
{
    [TestFixture]
    public class RefreshMovieServiceFixture : CoreTest<RefreshMovieService>
    {
        private MediaMetadata _movie;
        private Media _existingMovie;

        [SetUp]
        public void Setup()
        {
            _movie = Builder<MediaMetadata>.CreateNew()
                .With(s => s.Status = MovieStatusType.Released)
                .Build();

            _existingMovie = Builder<Media>.CreateNew()
                .With(s => s.MediaMetadata.Value.Status = MovieStatusType.Released)
                .Build();

            Mocker.GetMock<IMovieService>()
                  .Setup(s => s.GetMovie(_movie.Id))
                  .Returns(_existingMovie);

            Mocker.GetMock<IMovieMetadataService>()
                  .Setup(s => s.Get(_movie.Id))
                  .Returns(_movie);

            Mocker.GetMock<IProvideMovieInfo>()
                  .Setup(s => s.GetMovieInfo(It.IsAny<int>()))
                  .Callback<int>((i) => { throw new MovieNotFoundException(i); });

            Mocker.GetMock<IRootFolderService>()
                  .Setup(s => s.GetBestRootFolderPath(It.IsAny<string>()))
                  .Returns(string.Empty);
        }

        private void GivenNewMovieInfo(MediaMetadata movie)
        {
            Mocker.GetMock<IProvideMovieInfo>()
                  .Setup(s => s.GetMovieInfo(_movie.ForiegnId))
                  .Returns(new Tuple<MediaMetadata, List<Credit>>(movie, new List<Credit>()));
        }

        [Test]
        public void should_log_error_if_tmdb_id_not_found()
        {
            Subject.Execute(new RefreshMovieCommand(new List<int> { _movie.Id }));

            Mocker.GetMock<IMovieMetadataService>()
                .Verify(v => v.Upsert(It.Is<MediaMetadata>(s => s.Status == MovieStatusType.Deleted)), Times.Once());

            ExceptionVerification.ExpectedErrors(1);
        }

        [Test]
        public void should_update_if_tmdb_id_changed()
        {
            var newMovieInfo = _movie.JsonClone();
            newMovieInfo.ForiegnId = _movie.ForiegnId + 1;

            GivenNewMovieInfo(newMovieInfo);

            Subject.Execute(new RefreshMovieCommand(new List<int> { _movie.Id }));

            Mocker.GetMock<IMovieMetadataService>()
                .Verify(v => v.Upsert(It.Is<MediaMetadata>(s => s.ForiegnId == newMovieInfo.ForiegnId)));

            ExceptionVerification.ExpectedWarns(1);
        }

        [Test]
        public void should_mark_as_deleted_if_tmdb_id_not_found()
        {
            Subject.Execute(new RefreshMovieCommand(new List<int> { _movie.Id }));

            Mocker.GetMock<IMovieMetadataService>()
                .Verify(v => v.Upsert(It.Is<MediaMetadata>(s => s.Status == MovieStatusType.Deleted)), Times.Once());

            ExceptionVerification.ExpectedErrors(1);
        }

        [Test]
        public void should_not_remark_as_deleted_if_tmdb_id_not_found()
        {
            _movie.Status = MovieStatusType.Deleted;

            Subject.Execute(new RefreshMovieCommand(new List<int> { _movie.Id }));

            Mocker.GetMock<IMovieMetadataService>()
                .Verify(v => v.Upsert(It.IsAny<MediaMetadata>()), Times.Never());

            ExceptionVerification.ExpectedErrors(1);
        }
    }
}
