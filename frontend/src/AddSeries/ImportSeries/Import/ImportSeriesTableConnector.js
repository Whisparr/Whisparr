import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { queueLookupSeries, setImportSeriesValue } from 'Store/Actions/importSeriesActions';
import createAllSeriesSelector from 'Store/Selectors/createAllSeriesSelector';
import ImportSeriesTable from './ImportSeriesTable';

function createMapStateToProps() {
  return createSelector(
    (state) => state.addSeries,
    (state) => state.importSeries,
    (state) => state.app.dimensions,
    createAllSeriesSelector(),
    (addSeries, importSeries, dimensions, allSeries) => {
      return {
        defaultMonitor: addSeries.defaults.monitor,
        defaultQualityProfileId: addSeries.defaults.qualityProfileId,
        items: importSeries.items,
        isSmallScreen: dimensions.isSmallScreen,
        allSeries
      };
    }
  );
}

function createMapDispatchToProps(dispatch, props) {
  return {
    onSeriesLookup(name, path, relativePath) {
      dispatch(queueLookupSeries({
        name,
        path,
        relativePath,
        term: name
      }));
    },

    onSetImportSeriesValue(values) {
      dispatch(setImportSeriesValue(values));
    }
  };
}

export default connect(createMapStateToProps, createMapDispatchToProps)(ImportSeriesTable);
