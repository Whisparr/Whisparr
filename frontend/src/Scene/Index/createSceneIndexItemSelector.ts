import { createSelector } from 'reselect';
import Command from 'Commands/Command';
import { MOVIE_SEARCH, REFRESH_MOVIE } from 'Commands/commandNames';
import Movie from 'Movie/Movie';
import createExecutingCommandsSelector from 'Store/Selectors/createExecutingCommandsSelector';
import createMovieQualityProfileSelector from 'Store/Selectors/createMovieQualityProfileSelector';
import { createMovieSelectorForHook } from 'Store/Selectors/createMovieSelector';

function createSceneIndexItemSelector(sceneId: number) {
  return createSelector(
    createMovieSelectorForHook(sceneId),
    createMovieQualityProfileSelector(sceneId),
    createExecutingCommandsSelector(),
    (scene: Movie, qualityProfile, executingCommands: Command[]) => {
      const isRefreshingScene = executingCommands.some((command) => {
        return (
          command.name === REFRESH_MOVIE && command.body.movieId === sceneId
        );
      });

      const isSearchingScene = executingCommands.some((command) => {
        return (
          command.name === MOVIE_SEARCH && command.body.movieId === sceneId
        );
      });

      return {
        scene,
        qualityProfile,
        isRefreshingScene,
        isSearchingScene,
      };
    }
  );
}

export default createSceneIndexItemSelector;
