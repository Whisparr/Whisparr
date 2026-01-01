import _ from 'lodash';
import { createSelector } from 'reselect';
import movieEntities from 'Movie/movieEntities';

export function createMovieSelectorForHook(movieId) {
  return createSelector(
    (state) => state.movies.itemMap || {},
    (state) => state.movies.items || {},
    (state) => (state.moviePages ? state.moviePages.itemMap : {}) || {},
    (state) => (state.moviePages ? state.moviePages.items : []) || [],
    (state) => (state.scenePages ? state.scenePages.itemMap : {}) || {},
    (state) => (state.scenePages ? state.scenePages.items : []) || [],
    (itemMap, allMovies, pageItemMap, pageItems, scenePageItemMap, scenePageItems) => {
      if (!movieId) {
        return undefined;
      }

      const movieIndex = itemMap[movieId];

      if (movieIndex != null) {
        return allMovies[movieIndex];
      }

      const pageIndex = pageItemMap[movieId];

      if (pageIndex != null) {
        return pageItems[pageIndex];
      }

      const scenePageIndex = scenePageItemMap[movieId];

      if (scenePageIndex != null) {
        return scenePageItems[scenePageIndex];
      }

      return undefined;
    }
  );
}

export function createMovieByEntitySelector() {
  return createSelector(
    (state, { movieId }) => movieId,
    (state, { movieEntity = movieEntities.MOVIES }) => _.get(state, movieEntity, { items: [] }),
    (movieId, movies) => {
      return _.find(movies.items, { id: movieId });
    }
  );
}

function createMovieSelector() {
  return createSelector(
    (state, { movieId }) => movieId,
    (state) => state.movies.itemMap || {},
    (state) => state.movies.items || {},
    (state) => (state.moviePages ? state.moviePages.itemMap : {}) || {},
    (state) => (state.moviePages ? state.moviePages.items : []) || [],
    (state) => (state.scenePages ? state.scenePages.itemMap : {}) || {},
    (state) => (state.scenePages ? state.scenePages.items : []) || [],
    (movieId, itemMap, allMovies, pageItemMap, pageItems, scenePageItemMap, scenePageItems) => {
      const movieIndex = itemMap[movieId];

      if (movieIndex != null) {
        return allMovies[movieIndex];
      }

      const pageIndex = pageItemMap[movieId];

      if (pageIndex != null) {
        return pageItems[pageIndex];
      }

      const scenePageIndex = scenePageItemMap[movieId];

      if (scenePageIndex != null) {
        return scenePageItems[scenePageIndex];
      }

      return undefined;
    }
  );
}

export default createMovieSelector;
