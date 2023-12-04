import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createSceneSelector from 'Store/Selectors/createSceneSelector';
import createTagsSelector from 'Store/Selectors/createTagsSelector';
import SceneTags from './SceneTags';

function createMapStateToProps() {
  return createSelector(
    createSceneSelector(),
    createTagsSelector(),
    (scene, tagList) => {
      const tags = scene.tags
        .map((tagId) => tagList.find((tag) => tag.id === tagId))
        .filter((tag) => !!tag)
        .map((tag) => tag.label)
        .sort((a, b) => a.localeCompare(b));

      return {
        tags
      };
    }
  );
}

export default connect(createMapStateToProps)(SceneTags);
