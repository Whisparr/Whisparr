import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createDimensionsSelector from 'Store/Selectors/createDimensionsSelector';
import createExistingPerformerSelector from 'Store/Selectors/createExistingPerformerSelector';
import AddNewPerformerSearchResult from './AddNewPerformerSearchResult';

function createMapStateToProps() {
  return createSelector(
    createExistingPerformerSelector(),
    createDimensionsSelector(),
    (state) => state.queue.details.items,
    (state, { internalId }) => internalId,
    (state) => state.settings.safeForWorkMode,
    (isExistingPerformer, dimensions, queueItems, internalId, safeForWorkMode) => {
      const queueItem = queueItems.find((item) => internalId > 0 && item.movieId === internalId);
      return {
        existingPerformerId: internalId,
        isExistingPerformer,
        queueItem,
        isSmallScreen: dimensions.isSmallScreen,
        safeForWorkMode
      };
    }
  );
}

export default connect(createMapStateToProps)(AddNewPerformerSearchResult);
