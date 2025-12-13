import { createSelector } from 'reselect';
import AppState from 'App/State/AppState';

function createMultiMoviesSelector(movieIds: number[]) {
  return createSelector(
    (state: AppState) => state.movies.itemMap,
    (state: AppState) => state.movies.items,
    (itemMap, allMovies) => {
      if (itemMap === undefined || allMovies === undefined) {
        return [];
      }

      return movieIds.map((movieId) => allMovies[itemMap[movieId]]);
    }
  );
}

export default createMultiMoviesSelector;
