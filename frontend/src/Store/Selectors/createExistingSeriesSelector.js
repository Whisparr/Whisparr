import _ from 'lodash';
import { createSelector } from 'reselect';
import createAllSeriesSelector from './createAllSeriesSelector';

function createExistingSeriesSelector() {
  return createSelector(
    (state, { tpdbId }) => tpdbId,
    createAllSeriesSelector(),
    (tpdbId, series) => {
      return _.some(series, { tpdbId });
    }
  );
}

export default createExistingSeriesSelector;
