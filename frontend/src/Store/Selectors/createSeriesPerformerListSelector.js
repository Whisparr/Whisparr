import _ from 'lodash';
import { createSelector } from 'reselect';

function createSeriesPerformerListSelector() {
  return createSelector(
    (state, { tpdbId }) => tpdbId,
    (state) => state.settings.importLists.items,
    (tpdbId, importLists) => {
      const importListIds = _.reduce(importLists, (acc, list) => {
        if (list.implementation === 'TPDbPerformer') {
          const performerIdField = list.fields.find((field) => {
            return field.name === 'performerId';
          });

          if (performerIdField && parseInt(performerIdField.value) === tpdbId) {
            acc.push(list);
            return acc;
          }
        }

        return acc;
      }, []);

      if (importListIds.length === 0) {
        return undefined;
      }

      return importListIds[0];
    }
  );
}

export default createSeriesPerformerListSelector;
