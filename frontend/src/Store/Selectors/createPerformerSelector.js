import { createSelector } from 'reselect';

export function createPerformerSelectorForHook(performerId) {
  return createSelector(
    (state) => state.performers.itemMap,
    (state) => state.performers.items,
    (itemMap, allPerformers) => {
      return performerId ? allPerformers[itemMap[performerId]]: undefined;
    }
  );
}

function createPerformerSelector() {
  return createSelector(
    (state, { performerId }) => performerId,
    (state) => state.performers.itemMap,
    (state) => state.performers.items,
    (performerId, itemMap, allPerformers) => {
      return allPerformers[itemMap[performerId]];
    }
  );
}

export default createPerformerSelector;
