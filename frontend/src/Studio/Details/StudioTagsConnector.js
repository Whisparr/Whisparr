import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createStudioSelector from 'Store/Selectors/createStudioSelector';
import createTagsSelector from 'Store/Selectors/createTagsSelector';
import StudioTags from './StudioTags';

function createMapStateToProps() {
  return createSelector(
    createStudioSelector(),
    createTagsSelector(),
    (studio, tagList) => {
      const tags = studio.tags
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

export default connect(createMapStateToProps)(StudioTags);
