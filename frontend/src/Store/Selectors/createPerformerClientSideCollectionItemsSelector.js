import { createSelector, createSelectorCreator, defaultMemoize } from 'reselect';
import hasDifferentItemsOrOrder from 'Utilities/Object/hasDifferentItemsOrOrder';
import createClientSideCollectionSelector from './createClientSideCollectionSelector';

function createUnoptimizedSelector(uiSection) {
  return createSelector(
    createClientSideCollectionSelector('movies', uiSection),
    (movies) => {
      const items = movies.items.map((s) => s.credits.map((c) => {
        return {
          id: c.performer.foreignId,
          movieId: s.id,
          sortTitle: c.performer.name
        };
      })).flat();

      console.log(items);

      return {
        ...movies,
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

function createPerformerClientSideCollectionItemsSelector(uiSection) {
  return createMovieEqualSelector(
    createUnoptimizedSelector(uiSection),
    (performers) => performers
  );
}

export default createPerformerClientSideCollectionItemsSelector;
