import { some } from 'lodash';
import { createSelector } from 'reselect';
import AppState from 'App/State/AppState';
import createAllItemsSelector from './createAllItemsSelector';

function createExistingMovieSelector() {
  return createSelector(
    (_: AppState, { foreignId }: { foreignId: string }) => foreignId,
    createAllItemsSelector(),
    (foreignId, movies) => {
      return some(movies, { foreignId });
    }
  );
}

export default createExistingMovieSelector;
