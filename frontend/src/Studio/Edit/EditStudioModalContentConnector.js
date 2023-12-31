import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { saveStudio, setStudioValue } from 'Store/Actions/studioActions';
import createDimensionsSelector from 'Store/Selectors/createDimensionsSelector';
import createStudioSelector from 'Store/Selectors/createStudioSelector';
import selectSettings from 'Store/Selectors/selectSettings';
import EditStudioModalContent from './EditStudioModalContent';

function createIsPathChangingSelector() {
  return createSelector(
    (state) => state.studios.pendingChanges,
    createStudioSelector(),
    (pendingChanges, studio) => {
      const rootFolderPath = pendingChanges.rootFolderPath;

      if (rootFolderPath == null) {
        return false;
      }

      return studio.rootFolderPath !== rootFolderPath;
    }
  );
}

function createMapStateToProps() {
  return createSelector(
    (state) => state.studios,
    createStudioSelector(),
    createIsPathChangingSelector(),
    createDimensionsSelector(),
    (studiosState, studio, isPathChanging, dimensions) => {
      const {
        isSaving,
        saveError,
        pendingChanges
      } = studiosState;

      const studioSettings = {
        monitored: studio.monitored,
        qualityProfileId: studio.qualityProfileId,
        minimumAvailability: studio.minimumAvailability,
        rootFolderPath: studio.rootFolderPath,
        tags: studio.tags,
        searchOnAdd: studio.searchOnAdd
      };

      const settings = selectSettings(studioSettings, pendingChanges, saveError);

      return {
        title: studio.title,
        images: studio.images,
        overview: studio.overview,
        isSaving,
        saveError,
        isPathChanging,
        originalPath: studio.path,
        item: settings.settings,
        isSmallScreen: dimensions.isSmallScreen,
        ...settings
      };
    }
  );
}

const mapDispatchToProps = {
  dispatchSetStudioValue: setStudioValue,
  dispatchSaveStudio: saveStudio
};

class EditStudioModalContentConnector extends Component {

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
    this.props.dispatchSetStudioValue({ name, value });
  };

  onSavePress = () => {
    this.props.dispatchSaveStudio({
      id: this.props.studioId
    });
  };

  //
  // Render

  render() {
    return (
      <EditStudioModalContent
        {...this.props}
        onInputChange={this.onInputChange}
        onSavePress={this.onSavePress}
      />
    );
  }
}

EditStudioModalContentConnector.propTypes = {
  studioId: PropTypes.number,
  isSaving: PropTypes.bool.isRequired,
  saveError: PropTypes.object,
  dispatchSetStudioValue: PropTypes.func.isRequired,
  dispatchSaveStudio: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(EditStudioModalContentConnector);
