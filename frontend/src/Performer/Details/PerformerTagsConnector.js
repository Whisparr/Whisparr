import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createPerformerSelector from 'Store/Selectors/createPerformerSelector';
import createTagsSelector from 'Store/Selectors/createTagsSelector';
import PerformerTags from './PerformerTags';

function createMapStateToProps() {
  return createSelector(
    createPerformerSelector(),
    createTagsSelector(),
    (performer, tagList) => {
      const tags = performer.tags
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

export default connect(createMapStateToProps)(PerformerTags);
