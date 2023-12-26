import { createSelector } from 'reselect';

export function createPerformerSelector(movieId, performerId) {
  return createSelector(
    (state) => state.movies.itemMap,
    (state) => state.movies.items,
    (itemMap, allMovies) => {

      if (!performerId) {
        return undefined;
      }

      const movies = allMovies[itemMap[movieId]].credits.map((c) => c.performer).filter((p) => p.foreignId === performerId);

      return movies.length < 1 ? undefined : movies[0];
    }
  );
}
