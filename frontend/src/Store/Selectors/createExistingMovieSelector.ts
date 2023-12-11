import { some } from 'lodash';
import { createSelector } from 'reselect';
import AppState from 'App/State/AppState';
import createAllMoviesSelector from './createAllMoviesSelector';

function createExistingMovieSelector() {
  return createSelector(
    (_: AppState, { foreignId }: { foreignId: string }) => foreignId,
    createAllMoviesSelector(),
    (foreignId, movies) => {
      return some(movies, { foreignId });
    }
  );
}

export default createExistingMovieSelector;
