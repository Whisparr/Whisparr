import { createSelector, createSelectorCreator, defaultMemoize } from 'reselect';
import hasDifferentItemsOrOrder from 'Utilities/Object/hasDifferentItemsOrOrder';
import createClientSideCollectionSelector from './createClientSideCollectionSelector';

function createUnoptimizedSelector(uiSection) {
  return createSelector(
    createClientSideCollectionSelector('studios', uiSection),
    (studios) => {
      const items = studios.items.map((s) => {
        return {
          id: s.id,
          foreignId: s.foreignId,
          sortTitle: s.sortTitle
        };
      });

      return {
        ...studios,
        items
      };
    }
  );
}

function movieListEqual(a, b) {
  return hasDifferentItemsOrOrder(a, b);
}

const createMovieEqualSelector = createSelectorCreator(
  defaultMemoize,
  movieListEqual
);

function createStudioClientSideCollectionItemsSelector(uiSection) {
  return createMovieEqualSelector(
    createUnoptimizedSelector(uiSection),
    (studios) => studios
  );
}

export default createStudioClientSideCollectionItemsSelector;
