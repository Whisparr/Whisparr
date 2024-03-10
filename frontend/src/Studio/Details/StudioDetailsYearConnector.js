import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import * as commandNames from 'Commands/commandNames';
import { executeCommand } from 'Store/Actions/commandActions';
import { toggleMovieMonitored } from 'Store/Actions/movieActions';
import { setStudioScenesSort, setStudioScenesTableOption } from 'Store/Actions/studioScenesActions';
import createAllScenesSelector from 'Store/Selectors/createAllScenesSelector';
import createDimensionsSelector from 'Store/Selectors/createDimensionsSelector';
import createStudioSelector from 'Store/Selectors/createStudioSelector';
import StudioDetailsYear from './StudioDetailsYear';

function createMapStateToProps() {
  return createSelector(
    (state, { year }) => year,
    (state, { studioForeignId }) => studioForeignId,
    (state) => state.studioScenes,
    createAllScenesSelector(),
    createStudioSelector(),
    createDimensionsSelector(),
    (year, studioForeignId, studioScenes, scenes, studio, dimensions) => {

      const scenesInYear = scenes.filter((scene) => scene.studioForeignId === studioForeignId && scene.year === year);
      const sortedScenes =scenesInYear.sort(
        (a, b) => {
          if (studioScenes.sortDirection === 'ascending') {
            return new Date(a.releaseDate) - new Date(b.releaseDate);
          }
          return new Date(b.releaseDate) - new Date(a.releaseDate);
        });

      return {
        year,
        items: sortedScenes,
        columns: studioScenes.columns,
        sortKey: studioScenes.sortKey,
        sortDirection: studioScenes.sortDirection,
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
