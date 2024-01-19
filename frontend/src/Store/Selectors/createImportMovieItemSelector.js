import _ from 'lodash';
import { createSelector } from 'reselect';
import createAllItemsSelector from './createAllItemsSelector';

function createImportMovieItemSelector() {
  return createSelector(
    (state, { id }) => id,
    (state) => state.addMovie,
    (state) => state.importMovie,
    createAllItemsSelector(),
    (id, addMovie, importMovie, movies) => {
      const item = _.find(importMovie.items, { id }) || {};
      const selectedMovie = item && item.selectedMovie;
      const isExistingMovie = !!selectedMovie && _.some(movies, { foreignId: selectedMovie.foreignId });

      return {
        defaultMonitor: addMovie.movieDefaults.monitor,
        defaultQualityProfileId: addMovie.movieDefaults.qualityProfileId,
        ...item,
        isExistingMovie
      };
    }
  );
}

export default createImportMovieItemSelector;
