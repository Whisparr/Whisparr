import { createSelector } from 'reselect';
import Performer from 'Performer/Performer';
import { createPerformerSelectorForHook } from 'Store/Selectors/createPerformerSelector';

function createPerformerIndexItemSelector(performerId: number) {
  return createSelector(
    createPerformerSelectorForHook(performerId),
    (performer: Performer) => {
      return {
        performer,
      };
    }
  );
}

export default createPerformerIndexItemSelector;
