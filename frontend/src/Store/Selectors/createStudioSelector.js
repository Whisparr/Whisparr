import { createSelector } from 'reselect';

export function createStudioSelectorForHook(studioId) {
  return createSelector(
    (state) => state.studios.itemMap,
    (state) => state.studios.items,
    (itemMap, allStudios) => {
      return studioId ? allStudios[itemMap[studioId]]: undefined;
    }
  );
}

function createStudioSelector() {
  return createSelector(
    (state, { studioId }) => studioId,
    (state) => state.studios.itemMap,
    (state) => state.studios.items,
    (studioId, itemMap, allStudios) => {
      return allStudios[itemMap[studioId]];
    }
  );
}

export default createStudioSelector;
