
function getNewPerformer(performer, payload) {
  const {
    rootFolderPath,
    monitored,
    moviesMonitored,
    qualityProfileId,
    tags,
    afterDate,
    searchForMovie = false
  } = payload;

  const addOptions = {
    monitored,
    moviesMonitored,
    searchForMovie
  };

  performer.addOptions = addOptions;
  performer.monitored = monitored;
  performer.moviesMonitored = moviesMonitored;
  performer.qualityProfileId = qualityProfileId;
  performer.rootFolderPath = rootFolderPath;
  performer.afterDate = afterDate;
  performer.tags = tags;
  performer.searchOnAdd = searchForMovie;

  return performer;
}

export default getNewPerformer;
