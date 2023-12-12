import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createDimensionsSelector from 'Store/Selectors/createDimensionsSelector';
import createExclusionMovieSelector from 'Store/Selectors/createExclusionMovieSelector';
import createExistingMovieSelector from 'Store/Selectors/createExistingMovieSelector';
import AddNewMovieSearchResult from './AddNewMovieSearchResult';

function createMapStateToProps() {
  return createSelector(
    createExistingMovieSelector(),
    createExclusionMovieSelector(),
    createDimensionsSelector(),
    (state) => state.queue.details.items,
    (state, { internalId }) => internalId,
    (state) => state.settings.ui.item.movieRuntimeFormat,
    (state) => state.settings.safeForWorkMode,
    (isExistingMovie, isExclusionMovie, dimensions, queueItems, internalId, movieRuntimeFormat, safeForWorkMode) => {
      const queueItem = queueItems.find((item) => internalId > 0 && item.movieId === internalId);

      return {
        existingMovieId: internalId,
        isExistingMovie,
        isExclusionMovie,
        isSmallScreen: dimensions.isSmallScreen,
        queueItem,
        movieRuntimeFormat,
        safeForWorkMode
      };
    }
  );
}

export default connect(createMapStateToProps)(AddNewMovieSearchResult);
