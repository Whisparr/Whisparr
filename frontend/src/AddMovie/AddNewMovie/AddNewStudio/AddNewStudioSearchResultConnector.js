import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createDimensionsSelector from 'Store/Selectors/createDimensionsSelector';
import createExistingStudioSelector from 'Store/Selectors/createExistingStudioSelector';
import AddNewStudioSearchResult from './AddNewStudioSearchResult';

function createMapStateToProps() {
  return createSelector(
    createExistingStudioSelector(),
    createDimensionsSelector(),
    (state) => state.queue.details.items,
    (state, { internalId }) => internalId,
    (state) => state.settings.safeForWorkMode,
    (isExistingStudio, dimensions, queueItems, internalId, safeForWorkMode) => {
      const queueItem = queueItems.find((item) => internalId > 0 && item.studioId === internalId);
      return {
        existingStudioId: internalId,
        isExistingStudio,
        queueItem,
        isSmallScreen: dimensions.isSmallScreen,
        safeForWorkMode
      };
    }
  );
}

export default connect(createMapStateToProps)(AddNewStudioSearchResult);
