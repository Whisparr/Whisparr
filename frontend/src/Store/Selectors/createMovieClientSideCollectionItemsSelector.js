import { createSelector, createSelectorCreator, defaultMemoize } from 'reselect';
import hasDifferentItemsOrOrder from 'Utilities/Object/hasDifferentItemsOrOrder';
import createClientSideCollectionSelector from './createClientSideCollectionSelector';

function createUnoptimizedSelector(uiSection, mediaType) {
  return createSelector(
    createClientSideCollectionSelector('movies', uiSection),
    (state) => state.movies,
    (movies, allMovies) => {
      const totalItems = allMovies.items.filter((movie) => movie.itemType === mediaType).length;
      const items = movies.items.filter((movie) => movie.itemType === mediaType).map((s) => {
        const {
          id,
          sortTitle,
          collectionId
        } = s;

        return {
          id,
          sortTitle,
          collectionId
        };
      });

      return {
        ...movies,
        totalItems,
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

function createMovieClientSideCollectionItemsSelector(uiSection, mediaType) {
  return createMovieEqualSelector(
    createUnoptimizedSelector(uiSection, mediaType),
    (movies) => movies
  );
}

export default createMovieClientSideCollectionItemsSelector;
