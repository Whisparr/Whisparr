
function getNewPerformer(performer, payload) {
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

  performer.addOptions = addOptions;
  performer.monitored = monitor !== 'none';
  performer.qualityProfileId = qualityProfileId;
  performer.rootFolderPath = rootFolderPath;
  performer.tags = tags;
  performer.searchOnAdd = searchForMovie;

  return performer;
}

export default getNewPerformer;
