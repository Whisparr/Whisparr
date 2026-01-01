import { createSelector } from 'reselect';
import AppState from 'App/State/AppState';

const createScenePagedCollectionItemsSelector = createSelector(
  (state: AppState) => state.scenePages,
  (state: AppState) => state.sceneIndex,
  (scenePages, sceneIndex) => {
    const items = (scenePages.items ?? []).filter((item) => {
      const itemType = (
        (item as { itemType?: string }).itemType || ''
      ).toLowerCase();

      return itemType === 'scene';
    });

    return {
      ...sceneIndex,
      ...scenePages,
      items,
      totalItems: scenePages.totalRecords ?? items.length,
    };
  }
);

export default createScenePagedCollectionItemsSelector;
