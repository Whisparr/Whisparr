import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { addPerformer, setAddPerformerDefault } from 'Store/Actions/addMovieActions';
import createDimensionsSelector from 'Store/Selectors/createDimensionsSelector';
import createSystemStatusSelector from 'Store/Selectors/createSystemStatusSelector';
import selectSettings from 'Store/Selectors/selectSettings';
import AddNewPerformerModalContent from './AddNewPerformerModalContent';

function createMapStateToProps() {
  return createSelector(
    (state) => state.addMovie,
    createDimensionsSelector(),
    createSystemStatusSelector(),
    (state) => state.settings.safeForWorkMode,
    (addPerformerState, dimensions, systemStatus, safeForWorkMode) => {
      const {
        isAdding,
        addError,
        performerDefaults
      } = addPerformerState;

      const {
        settings,
        validationErrors,
        validationWarnings
      } = selectSettings(performerDefaults, {}, addError);

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
  setAddPerformerDefault,
  addPerformer
};

class AddNewPerformerModalContentConnector extends Component {

  //
  // Listeners

  onInputChange = ({ name, value }) => {
    this.props.setAddPerformerDefault({ [name]: value });
  };

  onAddPerformerPress = () => {
    const {
      foreignId,
      rootFolderPath,
      monitor,
      qualityProfileId,
      searchForMovie,
      tags
    } = this.props;

    this.props.addPerformer({
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
      <AddNewPerformerModalContent
        {...this.props}
        onInputChange={this.onInputChange}
        onAddPerformerPress={this.onAddPerformerPress}
      />
    );
  }
}

AddNewPerformerModalContentConnector.propTypes = {
  foreignId: PropTypes.string.isRequired,
  rootFolderPath: PropTypes.object,
  monitor: PropTypes.object.isRequired,
  qualityProfileId: PropTypes.object,
  searchForMovie: PropTypes.object.isRequired,
  tags: PropTypes.object.isRequired,
  onModalClose: PropTypes.func.isRequired,
  setAddPerformerDefault: PropTypes.func.isRequired,
  addPerformer: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(AddNewPerformerModalContentConnector);
