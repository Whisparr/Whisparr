import { createSelector } from 'reselect';
import appState from 'App/State/AppState';
import Studio from 'Studio/Studio';
import QualityProfile from 'typings/QualityProfile';
import { createStudioSelectorForHook } from './createStudioSelector';

function createStudioQualityProfileSelector(studioId: number) {
  return createSelector(
    (state: appState) => state.settings.qualityProfiles.items,
    createStudioSelectorForHook(studioId),
    (qualityProfiles: QualityProfile[], studio = {} as Studio) => {
      return qualityProfiles.find(
        (profile) => profile.id === studio.qualityProfileId
      );
    }
  );
}

export default createStudioQualityProfileSelector;
