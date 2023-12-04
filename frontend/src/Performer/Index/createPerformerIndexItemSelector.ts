import { createSelector } from 'reselect';
import Performer from 'Performer/Performer';
import createPerformerQualityProfileSelector from 'Store/Selectors/createPerformerQualityProfileSelector';
import { createPerformerSelectorForHook } from 'Store/Selectors/createPerformerSelector';

function createPerformerIndexItemSelector(performerId: number) {
  return createSelector(
    createPerformerSelectorForHook(performerId),
    createPerformerQualityProfileSelector(performerId),
    (performer: Performer, qualityProfile) => {
      return {
        performer,
        qualityProfile,
      };
    }
  );
}

export default createPerformerIndexItemSelector;
