import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { addSeries, setAddSeriesDefault } from 'Store/Actions/addSeriesActions';
import createDimensionsSelector from 'Store/Selectors/createDimensionsSelector';
import createSystemStatusSelector from 'Store/Selectors/createSystemStatusSelector';
import selectSettings from 'Store/Selectors/selectSettings';
import AddNewSeriesModalContent from './AddNewSeriesModalContent';

function createMapStateToProps() {
  return createSelector(
    (state) => state.addSeries,
    createDimensionsSelector(),
    createSystemStatusSelector(),
    (state) => state.settings.safeForWorkMode,
    (addSeriesState, dimensions, systemStatus, safeForWorkMode) => {
      const {
        isAdding,
        addError,
        defaults
      } = addSeriesState;

      const {
        settings,
        validationErrors,
        validationWarnings
      } = selectSettings(defaults, {}, addError);

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
  setAddSeriesDefault,
  addSeries
};

class AddNewSeriesModalContentConnector extends Component {

  //
  // Listeners

  onInputChange = ({ name, value }) => {
    this.props.setAddSeriesDefault({ [name]: value });
  };

  onAddSeriesPress = () => {
    const {
      tvdbId,
      rootFolderPath,
      monitor,
      qualityProfileId,
      searchForMissingEpisodes,
      searchForCutoffUnmetEpisodes,
      tags
    } = this.props;

    this.props.addSeries({
      tvdbId,
      rootFolderPath: rootFolderPath.value,
      monitor: monitor.value,
      qualityProfileId: qualityProfileId.value,
      searchForMissingEpisodes: searchForMissingEpisodes.value,
      searchForCutoffUnmetEpisodes: searchForCutoffUnmetEpisodes.value,
      tags: tags.value
    });
  };

  //
  // Render

  render() {
    return (
      <AddNewSeriesModalContent
        {...this.props}
        onInputChange={this.onInputChange}
        onAddSeriesPress={this.onAddSeriesPress}
      />
    );
  }
}

AddNewSeriesModalContentConnector.propTypes = {
  tvdbId: PropTypes.number.isRequired,
  rootFolderPath: PropTypes.object,
  monitor: PropTypes.object.isRequired,
  qualityProfileId: PropTypes.object,
  searchForMissingEpisodes: PropTypes.object.isRequired,
  searchForCutoffUnmetEpisodes: PropTypes.object.isRequired,
  tags: PropTypes.object.isRequired,
  onModalClose: PropTypes.func.isRequired,
  setAddSeriesDefault: PropTypes.func.isRequired,
  addSeries: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(AddNewSeriesModalContentConnector);
