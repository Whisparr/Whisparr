import { createSelector } from 'reselect';
import AppState from 'App/State/AppState';

function createAllMoviesSelector() {
  return createSelector(
    (state: AppState) => state.movies,
    (movies) => {
      return movies.items.filter((movie) => movie.itemType === 'movie');
    }
  );
}

export default createAllMoviesSelector;
