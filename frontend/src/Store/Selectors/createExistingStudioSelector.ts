import { some } from 'lodash';
import { createSelector } from 'reselect';
import AppState from 'App/State/AppState';

function createExistingStudioSelector() {
  return createSelector(
    (_: AppState, { foreignId }: { foreignId: string }) => foreignId,
    (state: AppState) => state.studios,
    (foreignId, studios) => {
      return some(studios.items, { foreignId });
    }
  );
}

export default createExistingStudioSelector;
