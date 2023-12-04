import { some } from 'lodash';
import { createSelector } from 'reselect';
import AppState from 'App/State/AppState';

function createExistingPerformerSelector() {
  return createSelector(
    (_: AppState, { foreignId }: { foreignId: string }) => foreignId,
    (state: AppState) => state.performers,
    (foreignId, performers) => {
      return some(performers.items, { foreignId });
    }
  );
}

export default createExistingPerformerSelector;
