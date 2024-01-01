import { createSelector } from 'reselect';
import appState from 'App/State/AppState';
import Studio from 'Studio/Studio';
import { createStudioSelectorForHook } from './createStudioSelector';

function createStudioQualityProfileSelector(studioId: number) {
  return createSelector(
    (state: appState) => state.settings.qualityProfiles.items,
    createStudioSelectorForHook(studioId),
    (qualityProfiles, studio = {} as Studio) => {
      return qualityProfiles.find(
        (profile) => profile.id === studio.qualityProfileId
      );
    }
  );
}

export default createStudioQualityProfileSelector;
