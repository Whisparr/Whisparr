import { createSelector } from 'reselect';
import AppState from 'App/State/AppState';

function createAllItemsSelector() {
  return createSelector(
    (state: AppState) => state.movies,
    (movies) => {
      return movies.items;
    }
  );
}

export default createAllItemsSelector;
