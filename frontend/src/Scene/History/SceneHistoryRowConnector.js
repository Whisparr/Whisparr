import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { fetchHistory, markAsFailed } from 'Store/Actions/historyActions';
import createSceneSelector from 'Store/Selectors/createSceneSelector';
import createUISettingsSelector from 'Store/Selectors/createUISettingsSelector';
import SceneHistoryRow from './SceneHistoryRow';

function createMapStateToProps() {
  return createSelector(
    createSceneSelector(),
    createUISettingsSelector(),
    (scene, uiSettings) => {
      return {
        scene,
        shortDateFormat: uiSettings.shortDateFormat,
        timeFormat: uiSettings.timeFormat
      };
    }
  );
}

const mapDispatchToProps = {
  fetchHistory,
  markAsFailed
};

export default connect(createMapStateToProps, mapDispatchToProps)(SceneHistoryRow);
