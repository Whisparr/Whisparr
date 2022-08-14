using NzbDrone.Core.Movies;

namespace NzbDrone.Core.Housekeeping.Housekeepers
{
    public class UpdateCleanTitleForMovies : IHousekeepingTask
    {
        private readonly IMediaRepository _movieRepository;

        public UpdateCleanTitleForMovies(IMediaRepository movieRepository)
        {
            _movieRepository = movieRepository;
        }

        public void Clean()
        {
            /*var movies = _movieRepository.All().ToList();

            movies.ForEach(m =>
            {
                m.CleanTitle = m.CleanTitle.CleanSeriesTitle();
                _movieRepository.Update(m);
            });*/
        }
    }
}
