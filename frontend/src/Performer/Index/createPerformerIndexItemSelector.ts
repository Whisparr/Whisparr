import { createSelector } from 'reselect';
import Performer from 'Performer/Performer';
import { createPerformerSelector } from 'Store/Selectors/createPerformerSelector';

function createPerformerIndexItemSelector(
  movieId: number,
  performerId: string
) {
  return createSelector(
    createPerformerSelector(movieId, performerId),
    (performer: Performer) => {
      return {
        performer,
      };
    }
  );
}

export default createPerformerIndexItemSelector;
