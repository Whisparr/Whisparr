import { push } from 'connected-react-router';
import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import * as commandNames from 'Commands/commandNames';
import { executeCommand } from 'Store/Actions/commandActions';
import { clearExtraFiles, fetchExtraFiles } from 'Store/Actions/extraFileActions';
import { clearQueueDetails, fetchQueueDetails } from 'Store/Actions/queueActions';
import { cancelFetchReleases, clearReleases } from 'Store/Actions/releaseActions';
import { toggleSceneMonitored } from 'Store/Actions/sceneActions';
import { clearSceneBlocklist, fetchSceneBlocklist } from 'Store/Actions/sceneBlocklistActions';
import { clearSceneCredits, fetchSceneCredits } from 'Store/Actions/sceneCreditsActions';
import { clearSceneFiles, fetchSceneFiles } from 'Store/Actions/sceneFileActions';
import { fetchImportListSchema } from 'Store/Actions/settingsActions';
import createAllScenesSelector from 'Store/Selectors/createAllScenesSelector';
import createCommandsSelector from 'Store/Selectors/createCommandsSelector';
import createDimensionsSelector from 'Store/Selectors/createDimensionsSelector';
import { findCommand, isCommandExecuting } from 'Utilities/Command';
import { registerPagePopulator, unregisterPagePopulator } from 'Utilities/pagePopulator';
import SceneDetails from './SceneDetails';

const selectSceneFiles = createSelector(
  (state) => state.sceneFiles,
  (sceneFiles) => {
    const {
      items,
      isFetching,
      isPopulated,
      error
    } = sceneFiles;

    const hasSceneFiles = !!items.length;

    const sizeOnDisk = items.map((item) => item.size).reduce((prev, curr) => prev + curr, 0);

    return {
      isSceneFilesFetching: isFetching,
      isSceneFilesPopulated: isPopulated,
      sceneFilesError: error,
      hasSceneFiles,
      sizeOnDisk
    };
  }
);

const selectSceneCredits = createSelector(
  (state) => state.sceneCredits,
  (sceneCredits) => {
    const {
      isFetching,
      isPopulated,
      error
    } = sceneCredits;

    return {
      isSceneCreditsFetching: isFetching,
      isSceneCreditsPopulated: isPopulated,
      sceneCreditsError: error
    };
  }
);

const selectExtraFiles = createSelector(
  (state) => state.extraFiles,
  (extraFiles) => {
    const {
      isFetching,
      isPopulated,
      error
    } = extraFiles;

    return {
      isExtraFilesFetching: isFetching,
      isExtraFilesPopulated: isPopulated,
      extraFilesError: error
    };
  }
);

function createMapStateToProps() {
  return createSelector(
    (state, { titleSlug }) => titleSlug,
    selectSceneFiles,
    selectSceneCredits,
    selectExtraFiles,
    createAllScenesSelector(),
    createCommandsSelector(),
    createDimensionsSelector(),
    (state) => state.queue.details.items,
    (state) => state.app.isSidebarVisible,
    (state) => state.settings.ui.item.sceneRuntimeFormat,
    (state) => state.settings.safeForWorkMode,
    (titleSlug, sceneFiles, sceneCredits, extraFiles, allScenes, commands, dimensions, queueItems, isSidebarVisible, sceneRuntimeFormat, safeForWorkMode) => {
      const sortedScenes = _.orderBy(allScenes, 'sortTitle');
      const sceneIndex = _.findIndex(sortedScenes, { titleSlug });
      const scene = sortedScenes[sceneIndex];

      if (!scene) {
        return {};
      }

      const {
        isSceneFilesFetching,
        isSceneFilesPopulated,
        sceneFilesError,
        hasSceneFiles,
        sizeOnDisk
      } = sceneFiles;

      const {
        isSceneCreditsFetching,
        isSceneCreditsPopulated,
        sceneCreditsError
      } = sceneCredits;

      const {
        isExtraFilesFetching,
        isExtraFilesPopulated,
        extraFilesError
      } = extraFiles;

      const previousScene = sortedScenes[sceneIndex - 1] || _.last(sortedScenes);
      const nextScene = sortedScenes[sceneIndex + 1] || _.first(sortedScenes);
      const isSceneRefreshing = isCommandExecuting(findCommand(commands, { name: commandNames.REFRESH_SCENE, sceneIds: [scene.id] }));
      const sceneRefreshingCommand = findCommand(commands, { name: commandNames.REFRESH_SCENE });
      const allScenesRefreshing = (
        isCommandExecuting(sceneRefreshingCommand) &&
        !sceneRefreshingCommand.body.sceneId
      );
      const isRefreshing = isSceneRefreshing || allScenesRefreshing;
      const isSearching = isCommandExecuting(findCommand(commands, { name: commandNames.SCENE_SEARCH, sceneIds: [scene.id] }));
      const isRenamingFiles = isCommandExecuting(findCommand(commands, { name: commandNames.RENAME_FILES, sceneId: scene.id }));
      const isRenamingSceneCommand = findCommand(commands, { name: commandNames.RENAME_SCENE });
      const isRenamingScene = (
        isCommandExecuting(isRenamingSceneCommand) &&
        isRenamingSceneCommand.body.sceneIds.indexOf(scene.id) > -1
      );

      const isFetching = isSceneFilesFetching || isSceneCreditsFetching || isExtraFilesFetching;
      const isPopulated = isSceneFilesPopulated && isSceneCreditsPopulated && isExtraFilesPopulated;
      const alternateTitles = _.reduce(scene.alternateTitles, (acc, alternateTitle) => {
        acc.push(alternateTitle.title);
        return acc;
      }, []);

      const queueItem = queueItems.find((item) => item.sceneId === scene.id);

      return {
        ...scene,
        alternateTitles,
        isSceneRefreshing,
        allScenesRefreshing,
        isRefreshing,
        isSearching,
        isRenamingFiles,
        isRenamingScene,
        isFetching,
        isPopulated,
        sceneFilesError,
        sceneCreditsError,
        extraFilesError,
        hasSceneFiles,
        sizeOnDisk,
        previousScene,
        nextScene,
        isSmallScreen: dimensions.isSmallScreen,
        isSidebarVisible,
        queueItem,
        sceneRuntimeFormat,
        safeForWorkMode
      };
    }
  );
}

function createMapDispatchToProps(dispatch, props) {
  return {
    dispatchFetchSceneFiles({ sceneId }) {
      dispatch(fetchSceneFiles({ sceneId }));
    },
    dispatchClearSceneFiles() {
      dispatch(clearSceneFiles());
    },
    dispatchFetchSceneCredits({ sceneId }) {
      dispatch(fetchSceneCredits({ sceneId }));
    },
    dispatchClearSceneCredits() {
      dispatch(clearSceneCredits());
    },
    dispatchFetchExtraFiles({ sceneId }) {
      dispatch(fetchExtraFiles({ sceneId }));
    },
    dispatchClearExtraFiles() {
      dispatch(clearExtraFiles());
    },
    dispatchClearReleases() {
      dispatch(clearReleases());
    },
    dispatchCancelFetchReleases() {
      dispatch(cancelFetchReleases());
    },
    dispatchFetchQueueDetails({ sceneId }) {
      dispatch(fetchQueueDetails({ sceneId }));
    },
    dispatchClearQueueDetails() {
      dispatch(clearQueueDetails());
    },
    dispatchFetchImportListSchema() {
      dispatch(fetchImportListSchema());
    },
    dispatchToggleSceneMonitored(payload) {
      dispatch(toggleSceneMonitored(payload));
    },
    dispatchExecuteCommand(payload) {
      dispatch(executeCommand(payload));
    },
    onGoToScene(titleSlug) {
      dispatch(push(`${window.Whisparr.urlBase}/scene/${titleSlug}`));
    },
    dispatchFetchSceneBlocklist({ sceneId }) {
      dispatch(fetchSceneBlocklist({ sceneId }));
    },
    dispatchClearSceneBlocklist() {
      dispatch(clearSceneBlocklist());
    }
  };
}

class SceneDetailsConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    registerPagePopulator(this.populate);
    this.populate();
  }

  componentDidUpdate(prevProps) {
    const {
      id,
      isSceneRefreshing,
      allScenesRefreshing,
      isRenamingFiles,
      isRenamingScene
    } = this.props;

    if (
      (prevProps.isSceneRefreshing && !isSceneRefreshing) ||
      (prevProps.allScenesRefreshing && !allScenesRefreshing) ||
      (prevProps.isRenamingFiles && !isRenamingFiles) ||
      (prevProps.isRenamingScene && !isRenamingScene)
    ) {
      this.populate();
    }

    // If the id has changed we need to clear the episodes/episode
    // files and fetch from the server.

    if (prevProps.id !== id) {
      this.unpopulate();
      this.populate();
    }
  }

  componentWillUnmount() {
    unregisterPagePopulator(this.populate);
    this.unpopulate();
  }

  //
  // Control

  populate = () => {
    const sceneId = this.props.id;

    this.props.dispatchFetchSceneFiles({ sceneId });
    this.props.dispatchFetchSceneBlocklist({ sceneId });
    this.props.dispatchFetchExtraFiles({ sceneId });
    this.props.dispatchFetchSceneCredits({ sceneId });
    this.props.dispatchFetchQueueDetails({ sceneId });
    this.props.dispatchFetchImportListSchema();
  };

  unpopulate = () => {
    this.props.dispatchCancelFetchReleases();
    this.props.dispatchClearSceneBlocklist();
    this.props.dispatchClearSceneFiles();
    this.props.dispatchClearExtraFiles();
    this.props.dispatchClearSceneCredits();
    this.props.dispatchClearQueueDetails();
    this.props.dispatchClearReleases();
  };

  //
  // Listeners

  onMonitorTogglePress = (monitored) => {
    this.props.dispatchToggleSceneMonitored({
      sceneId: this.props.id,
      monitored
    });
  };

  onRefreshPress = () => {
    this.props.dispatchExecuteCommand({
      name: commandNames.REFRESH_SCENE,
      sceneIds: [this.props.id]
    });
  };

  onSearchPress = () => {
    this.props.dispatchExecuteCommand({
      name: commandNames.SCENE_SEARCH,
      sceneIds: [this.props.id]
    });
  };

  //
  // Render

  render() {
    return (
      <SceneDetails
        {...this.props}
        onMonitorTogglePress={this.onMonitorTogglePress}
        onRefreshPress={this.onRefreshPress}
        onSearchPress={this.onSearchPress}
      />
    );
  }
}

SceneDetailsConnector.propTypes = {
  id: PropTypes.number.isRequired,
  titleSlug: PropTypes.string.isRequired,
  isSceneRefreshing: PropTypes.bool.isRequired,
  allScenesRefreshing: PropTypes.bool.isRequired,
  isRefreshing: PropTypes.bool.isRequired,
  isRenamingFiles: PropTypes.bool.isRequired,
  isRenamingScene: PropTypes.bool.isRequired,
  isSmallScreen: PropTypes.bool.isRequired,
  dispatchFetchSceneFiles: PropTypes.func.isRequired,
  dispatchClearSceneFiles: PropTypes.func.isRequired,
  dispatchFetchExtraFiles: PropTypes.func.isRequired,
  dispatchClearExtraFiles: PropTypes.func.isRequired,
  dispatchFetchSceneCredits: PropTypes.func.isRequired,
  dispatchClearSceneCredits: PropTypes.func.isRequired,
  dispatchClearReleases: PropTypes.func.isRequired,
  dispatchCancelFetchReleases: PropTypes.func.isRequired,
  dispatchToggleSceneMonitored: PropTypes.func.isRequired,
  dispatchFetchQueueDetails: PropTypes.func.isRequired,
  dispatchClearQueueDetails: PropTypes.func.isRequired,
  dispatchFetchImportListSchema: PropTypes.func.isRequired,
  dispatchExecuteCommand: PropTypes.func.isRequired,
  dispatchFetchSceneBlocklist: PropTypes.func.isRequired,
  dispatchClearSceneBlocklist: PropTypes.func.isRequired,
  onGoToScene: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, createMapDispatchToProps)(SceneDetailsConnector);
