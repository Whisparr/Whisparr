import { createSelector } from 'reselect';
import { createStudioSelector } from 'Store/Selectors/createStudioSelector';
import Studio from 'Studio/Studio';

function createStudioIndexItemSelector(studioId: number) {
  return createSelector(createStudioSelector(studioId), (studio: Studio) => {
    return {
      studio,
    };
  });
}

export default createStudioIndexItemSelector;
