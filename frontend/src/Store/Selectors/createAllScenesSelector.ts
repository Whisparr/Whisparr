import { createSelector } from 'reselect';
import AppState from 'App/State/AppState';

function createAllScenesSelector() {
  return createSelector(
    (state: AppState) => state.movies,
    (movies) => {
      return movies.items.filter((movie) => movie.itemType === 'scene');
    }
  );
}

export default createAllScenesSelector;
