import { createSelector } from 'reselect';

export function createPerformerSelector(performerId) {
  return createSelector(
    (state) => state.performers.itemMap,
    (state) => state.performers.items,
    (itemMap, allPerformers) => {
      return performerId ? allPerformers[itemMap[performerId]]: undefined;
    }
  );
}
