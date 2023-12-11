import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { saveMovie, setMovieValue } from 'Store/Actions/movieActions';
import createMovieSelector from 'Store/Selectors/createMovieSelector';
import selectSettings from 'Store/Selectors/selectSettings';
import EditSceneModalContent from './EditSceneModalContent';

function createIsPathChangingSelector() {
  return createSelector(
    (state) => state.movies.pendingChanges,
    createMovieSelector(),
    (pendingChanges, scene) => {
      const path = pendingChanges.path;

      if (path == null) {
        return false;
      }

      return scene.path !== path;
    }
  );
}

function createMapStateToProps() {
  return createSelector(
    (state) => state.movies,
    createMovieSelector(),
    createIsPathChangingSelector(),
    (scenesState, scene, isPathChanging) => {
      const {
        isSaving,
        saveError,
        pendingChanges
      } = scenesState;

      const sceneSettings = {
        monitored: scene.monitored,
        qualityProfileId: scene.qualityProfileId,
        path: scene.path,
        tags: scene.tags
      };

      const settings = selectSettings(sceneSettings, pendingChanges, saveError);

      return {
        title: scene.title,
        isSaving,
        saveError,
        isPathChanging,
        originalPath: scene.path,
        item: settings.settings,
        ...settings
      };
    }
  );
}

const mapDispatchToProps = {
  dispatchSetSceneValue: setMovieValue,
  dispatchSaveScene: saveMovie
};

class EditSceneModalContentConnector extends Component {

  //
  // Lifecycle

  componentDidUpdate(prevProps, prevState) {
    if (prevProps.isSaving && !this.props.isSaving && !this.props.saveError) {
      this.props.onModalClose();
    }
  }

  //
  // Listeners

  onInputChange = ({ name, value }) => {
    this.props.dispatchSetSceneValue({ name, value });
  };

  onSavePress = (moveFiles) => {
    this.props.dispatchSaveScene({
      id: this.props.sceneId,
      moveFiles
    });
  };

  //
  // Render

  render() {
    return (
      <EditSceneModalContent
        {...this.props}
        onInputChange={this.onInputChange}
        onSavePress={this.onSavePress}
        onMoveScenePress={this.onMoveScenePress}
      />
    );
  }
}

EditSceneModalContentConnector.propTypes = {
  sceneId: PropTypes.number,
  isSaving: PropTypes.bool.isRequired,
  saveError: PropTypes.object,
  dispatchSetSceneValue: PropTypes.func.isRequired,
  dispatchSaveScene: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(EditSceneModalContentConnector);
