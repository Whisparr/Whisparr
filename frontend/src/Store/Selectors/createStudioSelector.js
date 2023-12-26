import { createSelector } from 'reselect';

export function createStudioSelector(studioId) {
  return createSelector(
    (state) => state.studios.itemMap,
    (state) => state.studios.items,
    (itemMap, allStudios) => {
      return studioId ? allStudios[itemMap[studioId]]: undefined;
    }
  );
}
