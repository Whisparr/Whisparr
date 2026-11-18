import { useMemo } from 'react';
import { useSelector } from 'react-redux';
import AppState from 'App/State/AppState';
import createAllSeriesSelector from 'Store/Selectors/createAllSeriesSelector';

function useQualityProfileInUse(id: number | undefined) {
  const series = useSelector(createAllSeriesSelector());
  const importLists = useSelector(
    (state: AppState) => state.settings.importLists.items
  );

  return useMemo(() => {
    if (!id) {
      return {
        seriesCount: 0,
        importListCount: 0,
      };
    }

    return {
      seriesCount: series.filter((s) => s.qualityProfileId === id).length,
      importListCount: importLists.filter(
        (list) => list.qualityProfileId === id
      ).length,
    };
  }, [id, series, importLists]);
}

export default useQualityProfileInUse;
