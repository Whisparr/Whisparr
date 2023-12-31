import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createDimensionsSelector from 'Store/Selectors/createDimensionsSelector';
import createExistingPerformerSelector from 'Store/Selectors/createExistingPerformerSelector';
import AddNewPerformerSearchResult from './AddNewPerformerSearchResult';

function createMapStateToProps() {
  return createSelector(
    createExistingPerformerSelector(),
    createDimensionsSelector(),
    (state, { internalId }) => internalId,
    (state) => state.settings.ui.item.movieRuntimeFormat,
    (state) => state.settings.safeForWorkMode,
    (isExistingPerformer, dimensions, internalId, movieRuntimeFormat, safeForWorkMode) => {
      return {
        existingMovieId: internalId,
        isExistingPerformer,
        isSmallScreen: dimensions.isSmallScreen,
        movieRuntimeFormat,
        safeForWorkMode
      };
    }
  );
}

export default connect(createMapStateToProps)(AddNewPerformerSearchResult);
