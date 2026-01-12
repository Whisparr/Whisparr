
function getNewMovie(movie, payload) {
  const {
    rootFolderPath,
    monitored,
    qualityProfileId,
    tags,
    searchForMovie = false
  } = payload;

  const addOptions = {
    monitored,
    searchForMovie
  };

  movie.addOptions = addOptions;
  movie.monitored = monitored;
  movie.qualityProfileId = qualityProfileId;
  movie.rootFolderPath = rootFolderPath;
  movie.tags = tags;

  return movie;
}

export default getNewMovie;
