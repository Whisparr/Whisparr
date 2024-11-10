import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import * as commandNames from 'Commands/commandNames';
import { executeCommand } from 'Store/Actions/commandActions';
import { toggleMovieMonitored } from 'Store/Actions/movieActions';
import { setStudioScenesSort, setStudioScenesTableOption } from 'Store/Actions/studioScenesActions';
import createClientSideCollectionSelector from 'Store/Selectors/createClientSideCollectionSelector';
import createDimensionsSelector from 'Store/Selectors/createDimensionsSelector';
import createStudioSelector from 'Store/Selectors/createStudioSelector';
import StudioDetailsYear from './StudioDetailsYear';

function createMapStateToProps() {
  return createSelector(
    (state, { year }) => year,
    (state, { studioForeignId }) => studioForeignId,
    createClientSideCollectionSelector('movies', 'studioScenes'),
    createStudioSelector(),
    createDimensionsSelector(),
    (year, studioForeignId, scenes, studio, dimensions) => {

      const scenesInYear = scenes.items.filter((scene) => scene.studioForeignId === studioForeignId && scene.year === year);

      return {
        year,
        items: scenesInYear,
        columns: scenes.columns,
        sortKey: scenes.sortKey,
        sortDirection: scenes.sortDirection,
        studioMonitored: studio.monitored,
        path: studio.path,
        isSmallScreen: dimensions.isSmallScreen,
        isSearching: false
      };
    }
  );
}

const mapDispatchToProps = {
  setStudioScenesSort,
  setStudioScenesTableOption,
  toggleMovieMonitored,
  executeCommand
};

class StudioDetailsYearConnector extends Component {

  onTableOptionChange = (payload) => {
    this.props.setStudioScenesTableOption(payload);
  };

  onMonitorMoviePress = (movieId, monitored) => {
    this.props.toggleMovieMonitored({
      movieId,
      monitored
    });
  };

  onSearchPress = () => {
    this.props.executeCommand({
      name: commandNames.STUDIO_SEARCH,
      studioIds: [this.props.studioId],
      years: [this.props.year]
    });
  };

  onSortPress = (sortKey, sortDirection) => {
    this.props.setStudioScenesSort({
      sortKey,
      sortDirection
    });
  };

  //
  // Render

  render() {
    return (
      <StudioDetailsYear
        {...this.props}
        onTableOptionChange={this.onTableOptionChange}
        onSortPress={this.onSortPress}
        onSearchPress={this.onSearchPress}
        onMonitorMoviePress={this.onMonitorMoviePress}
      />
    );
  }
}

StudioDetailsYearConnector.propTypes = {
  studioId: PropTypes.number.isRequired,
  year: PropTypes.number.isRequired,
  setStudioScenesTableOption: PropTypes.func.isRequired,
  setStudioScenesSort: PropTypes.func.isRequired,
  toggleMovieMonitored: PropTypes.func.isRequired,
  executeCommand: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(StudioDetailsYearConnector);
