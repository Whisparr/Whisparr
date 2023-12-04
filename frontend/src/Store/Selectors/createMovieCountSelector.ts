import { createSelector } from 'reselect';
import AppState from 'App/State/AppState';
import createAllItemsSelector from './createAllItemsSelector';

function createMovieCountSelector() {
  return createSelector(
    createAllItemsSelector(),
    (state: AppState) => state.movies.error,
    (state: AppState) => state.movies.isFetching,
    (state: AppState) => state.movies.isPopulated,
    (movies, error, isFetching, isPopulated) => {
      return {
        count: movies.length,
        error,
        isFetching,
        isPopulated,
      };
    }
  );
}

export default createMovieCountSelector;
