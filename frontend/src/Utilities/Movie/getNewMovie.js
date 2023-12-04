
function getNewMovie(movie, payload) {
  const {
    rootFolderPath,
    monitor,
    qualityProfileId,
    tags,
    searchForMovie = false
  } = payload;

  const addOptions = {
    monitor,
    searchForMovie
  };

  movie.addOptions = addOptions;
  movie.monitored = monitor !== 'none';
  movie.qualityProfileId = qualityProfileId;
  movie.rootFolderPath = rootFolderPath;
  movie.tags = tags;

  return movie;
}

export default getNewMovie;
