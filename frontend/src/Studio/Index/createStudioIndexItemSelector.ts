import { createSelector } from 'reselect';
import { createStudioSelectorForHook } from 'Store/Selectors/createStudioSelector';
import Studio from 'Studio/Studio';

function createStudioIndexItemSelector(studioId: number) {
  return createSelector(
    createStudioSelectorForHook(studioId),
    (studio: Studio) => {
      return {
        studio,
      };
    }
  );
}

export default createStudioIndexItemSelector;
