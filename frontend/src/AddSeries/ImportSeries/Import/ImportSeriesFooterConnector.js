import _ from 'lodash';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { cancelLookupSeries, lookupUnsearchedSeries } from 'Store/Actions/importSeriesActions';
import ImportSeriesFooter from './ImportSeriesFooter';

function isMixed(items, selectedIds, defaultValue, key) {
  return _.some(items, (series) => {
    return selectedIds.indexOf(series.id) > -1 && series[key] !== defaultValue;
  });
}

function createMapStateToProps() {
  return createSelector(
    (state) => state.addSeries,
    (state) => state.importSeries,
    (state, { selectedIds }) => selectedIds,
    (addSeries, importSeries, selectedIds) => {
      const {
        monitor: defaultMonitor,
        qualityProfileId: defaultQualityProfileId
      } = addSeries.defaults;

      const {
        isLookingUpSeries,
        isImporting,
        items,
        importError
      } = importSeries;

      const isMonitorMixed = isMixed(items, selectedIds, defaultMonitor, 'monitor');
      const isQualityProfileIdMixed = isMixed(items, selectedIds, defaultQualityProfileId, 'qualityProfileId');
      const hasUnsearchedItems = !isLookingUpSeries && items.some((item) => !item.isPopulated);

      return {
        selectedCount: selectedIds.length,
        isLookingUpSeries,
        isImporting,
        defaultMonitor,
        defaultQualityProfileId,
        isMonitorMixed,
        isQualityProfileIdMixed,
        importError,
        hasUnsearchedItems
      };
    }
  );
}

const mapDispatchToProps = {
  onLookupPress: lookupUnsearchedSeries,
  onCancelLookupPress: cancelLookupSeries
};

export default connect(createMapStateToProps, mapDispatchToProps)(ImportSeriesFooter);
