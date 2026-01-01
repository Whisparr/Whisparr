import { createSelector } from 'reselect';
import AppState from 'App/State/AppState';

const createMoviePagedCollectionItemsSelector = createSelector(
  (state: AppState) => state.moviePages,
  (state: AppState) => state.movieIndex,
  (moviePages, movieIndex) => {
    const items = (moviePages.items ?? []).filter((item) => {
      const itemType = (
        (item as { itemType?: string }).itemType || ''
      ).toLowerCase();

      return itemType === 'movie';
    });

    return {
      ...movieIndex,
      ...moviePages,
      items,
      totalItems: moviePages.totalRecords ?? items.length,
    };
  }
);

export default createMoviePagedCollectionItemsSelector;
