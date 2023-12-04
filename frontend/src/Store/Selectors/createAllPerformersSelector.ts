import { createSelector } from 'reselect';
import AppState from 'App/State/AppState';

function createAllPerformersSelector() {
  return createSelector(
    (state: AppState) => state.performers,
    (performers) => {
      return performers.items;
    }
  );
}

export default createAllPerformersSelector;
