import { createSelector } from 'reselect';
import Performer from 'Performer/Performer';
import { createPerformerSelector } from 'Store/Selectors/createPerformerSelector';

function createPerformerIndexItemSelector(performerId: number) {
  return createSelector(
    createPerformerSelector(performerId),
    (performer: Performer) => {
      return {
        performer,
      };
    }
  );
}

export default createPerformerIndexItemSelector;
