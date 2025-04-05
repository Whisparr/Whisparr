import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { addStudio, setAddStudioDefault } from 'Store/Actions/addMovieActions';
import createDimensionsSelector from 'Store/Selectors/createDimensionsSelector';
import createSystemStatusSelector from 'Store/Selectors/createSystemStatusSelector';
import selectSettings from 'Store/Selectors/selectSettings';
import AddNewStudioModalContent from './AddNewStudioModalContent';

function createMapStateToProps() {
  return createSelector(
    (state) => state.addMovie,
    createDimensionsSelector(),
    createSystemStatusSelector(),
    (state) => state.settings.safeForWorkMode,
    (addStudioState, dimensions, systemStatus, safeForWorkMode) => {
      const {
        isAdding,
        addError,
        studioDefaults
      } = addStudioState;

      const {
        settings,
        validationErrors,
        validationWarnings
      } = selectSettings(studioDefaults, {}, addError);

      return {
        isAdding,
        addError,
        isSmallScreen: dimensions.isSmallScreen,
        validationErrors,
        validationWarnings,
        isWindows: systemStatus.isWindows,
        safeForWorkMode,
        ...settings
      };
    }
  );
}
const mapDispatchToProps = {
  setAddStudioDefault,
  addStudio
};

class AddNewStudioModalContentConnector extends Component {

  //
  // Listeners

  onInputChange = ({ name, value }) => {
    this.props.setAddStudioDefault({ [name]: value });
  };

  onAddStudioPress = () => {
    const {
      foreignId,
      rootFolderPath,
      monitor,
      qualityProfileId,
      searchForMovie,
      tags
    } = this.props;

    this.props.addStudio({
      foreignId,
      rootFolderPath: rootFolderPath.value,
      monitor: monitor.value,
      qualityProfileId: qualityProfileId.value,
      searchForMovie: searchForMovie.value,
      tags: tags.value
    });
  };

  //
  // Render

  render() {
    return (
      <AddNewStudioModalContent
        {...this.props}
        onInputChange={this.onInputChange}
        onAddStudioPress={this.onAddStudioPress}
      />
    );
  }
}

AddNewStudioModalContentConnector.propTypes = {
  foreignId: PropTypes.string.isRequired,
  rootFolderPath: PropTypes.object,
  monitor: PropTypes.object.isRequired,
  qualityProfileId: PropTypes.object,
  searchForMovie: PropTypes.object.isRequired,
  tags: PropTypes.object.isRequired,
  onModalClose: PropTypes.func.isRequired,
  setAddStudioDefault: PropTypes.func.isRequired,
  addStudio: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(AddNewStudioModalContentConnector);
