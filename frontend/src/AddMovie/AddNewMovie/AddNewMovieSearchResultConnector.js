import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createDimensionsSelector from 'Store/Selectors/createDimensionsSelector';
import createExistingMovieSelector from 'Store/Selectors/createExistingMovieSelector';
import AddNewMovieSearchResult from './AddNewMovieSearchResult';

function createMapStateToProps() {
  return createSelector(
    createExistingMovieSelector(),
    createDimensionsSelector(),
    (state) => state.settings.safeForWorkMode,
    (isExistingMovie, dimensions, safeForWorkMode) => {
      return {
        isExistingMovie,
        isSmallScreen: dimensions.isSmallScreen,
        safeForWorkMode
      };
    }
  );
}

export default connect(createMapStateToProps)(AddNewMovieSearchResult);
