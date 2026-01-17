
function getNewSeries(series, payload) {
  const {
    rootFolderPath,
    monitor,
    monitorNewItems,
    seriesType,
    qualityProfileId,
    tags,
    searchForMissingEpisodes = false,
    searchForCutoffUnmetEpisodes = false
  } = payload;

  const addOptions = {
    monitor,
    searchForMissingEpisodes,
    searchForCutoffUnmetEpisodes
  };

  series.addOptions = addOptions;
  series.monitored = true;
  series.monitorNewItems = monitorNewItems;
  series.seriesType = seriesType;
  series.qualityProfileId = qualityProfileId;
  series.rootFolderPath = rootFolderPath;
  series.tags = tags;

  return series;
}

export default getNewSeries;
