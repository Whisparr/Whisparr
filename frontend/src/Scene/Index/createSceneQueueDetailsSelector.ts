import { createSelector } from 'reselect';
import AppState from 'App/State/AppState';

export interface SceneQueueDetails {
  count: number;
}

function createSceneQueueDetailsSelector(sceneId: number) {
  return createSelector(
    (state: AppState) => state.queue.details.items,
    (queueItems) => {
      return queueItems.reduce(
        (acc: SceneQueueDetails, item) => {
          if (
            item.trackedDownloadState === 'imported' ||
            item.movieId !== sceneId
          ) {
            return acc;
          }

          acc.count++;

          return acc;
        },
        {
          count: 0,
        }
      );
    }
  );
}

export default createSceneQueueDetailsSelector;
