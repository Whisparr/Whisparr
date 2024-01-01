import { createSelector } from 'reselect';
import createStudioQualityProfileSelector from 'Store/Selectors/createStudioQualityProfileSelector';
import { createStudioSelectorForHook } from 'Store/Selectors/createStudioSelector';
import Studio from 'Studio/Studio';

function createStudioIndexItemSelector(studioId: number) {
  return createSelector(
    createStudioSelectorForHook(studioId),
    createStudioQualityProfileSelector(studioId),
    (studio: Studio, qualityProfile) => {
      return {
        studio,
        qualityProfile,
      };
    }
  );
}

export default createStudioIndexItemSelector;
