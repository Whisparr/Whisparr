import { createSelector } from 'reselect';
import AppState from 'App/State/AppState';

function createAllStudiosSelector() {
  return createSelector(
    (state: AppState) => state.studios,
    (studios) => {
      return studios.items;
    }
  );
}

export default createAllStudiosSelector;
