import { createSelector } from 'reselect';
import appState from 'App/State/AppState';
import Performer from 'Performer/Performer';
import { createPerformerSelectorForHook } from './createPerformerSelector';

function createPerformerQualityProfileSelector(performerId: number) {
  return createSelector(
    (state: appState) => state.settings.qualityProfiles.items,
    createPerformerSelectorForHook(performerId),
    (qualityProfiles, performer = {} as Performer) => {
      return qualityProfiles.find(
        (profile) => profile.id === performer.qualityProfileId
      );
    }
  );
}

export default createPerformerQualityProfileSelector;
