import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { savePerformer, setPerformerValue } from 'Store/Actions/performerActions';
import createDimensionsSelector from 'Store/Selectors/createDimensionsSelector';
import createPerformerSelector from 'Store/Selectors/createPerformerSelector';
import selectSettings from 'Store/Selectors/selectSettings';
import EditPerformerModalContent from './EditPerformerModalContent';

function createIsPathChangingSelector() {
  return createSelector(
    (state) => state.performers.pendingChanges,
    createPerformerSelector(),
    (pendingChanges, performer) => {
      const rootFolderPath = pendingChanges.rootFolderPath;

      if (rootFolderPath == null) {
        return false;
      }

      return performer.rootFolderPath !== rootFolderPath;
    }
  );
}

function createMapStateToProps() {
  return createSelector(
    (state) => state.performers,
    createPerformerSelector(),
    createIsPathChangingSelector(),
    createDimensionsSelector(),
    (state) => state.settings.safeForWorkMode,
    (performersState, performer, isPathChanging, dimensions, safeForWorkMode) => {
      const {
        isSaving,
        saveError,
        pendingChanges
      } = performersState;

      const movieSettings = {
        monitored: performer.monitored,
        qualityProfileId: performer.qualityProfileId,
        minimumAvailability: performer.minimumAvailability,
        rootFolderPath: performer.rootFolderPath,
        tags: performer.tags,
        searchOnAdd: performer.searchOnAdd
      };

      const settings = selectSettings(movieSettings, pendingChanges, saveError);

      return {
        fullName: performer.fullName,
        images: performer.images,
        overview: performer.overview,
        isSaving,
        saveError,
        isPathChanging,
        originalPath: performer.path,
        item: settings.settings,
        isSmallScreen: dimensions.isSmallScreen,
        safeForWorkMode,
        ...settings
      };
    }
  );
}

const mapDispatchToProps = {
  dispatchSetPerformerValue: setPerformerValue,
  dispatchSavePerformer: savePerformer
};

class EditPerformerModalContentConnector extends Component {

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
    this.props.dispatchSetPerformerValue({ name, value });
  };

  onSavePress = () => {
    this.props.dispatchSavePerformer({
      id: this.props.performerId
    });
  };

  //
  // Render

  render() {
    return (
      <EditPerformerModalContent
        {...this.props}
        onInputChange={this.onInputChange}
        onSavePress={this.onSavePress}
      />
    );
  }
}

EditPerformerModalContentConnector.propTypes = {
  performerId: PropTypes.number,
  isSaving: PropTypes.bool.isRequired,
  saveError: PropTypes.object,
  dispatchSetPerformerValue: PropTypes.func.isRequired,
  dispatchSavePerformer: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(EditPerformerModalContentConnector);
