import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createDimensionsSelector from 'Store/Selectors/createDimensionsSelector';
import DiscoverMoviePoster from './DiscoverMoviePoster';

function createMapStateToProps() {
  return createSelector(
    createDimensionsSelector(),
    (state) => state.settings.safeForWorkMode,
    (dimensions, safeForWork) => {
      return {
        isSmallScreen: dimensions.isSmallScreen,
        safeForWork
      };
    }
  );
}

export default connect(createMapStateToProps)(DiscoverMoviePoster);
