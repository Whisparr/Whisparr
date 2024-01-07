import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { executeCommand } from 'Store/Actions/commandActions';
import { toggleMovieMonitored } from 'Store/Actions/movieActions';
import { setPerformerScenesSort, setPerformerScenesTableOption } from 'Store/Actions/performerScenesActions';
import { toggleStudioMonitored } from 'Store/Actions/studioActions';
import createAllScenesSelector from 'Store/Selectors/createAllScenesSelector';
import createDimensionsSelector from 'Store/Selectors/createDimensionsSelector';
import createPerformerSelector from 'Store/Selectors/createPerformerSelector';
import PerformerDetailsStudio from './PerformerDetailsStudio';

function createStudioByForeignIdSelector() {
  return createSelector(
    (state, { studioForeignId }) => studioForeignId,
    (state) => state.studios,
    (studioForeignId, studios) => {
      const selectedStudio = studios.items.find((studio) => studio.foreignId === studioForeignId);

      return {
        ...selectedStudio
      };
    }
  );
}

function createMapStateToProps() {
  return createSelector(
    (state, { studioForeignId }) => studioForeignId,
    (state) => state.performerScenes,
    createStudioByForeignIdSelector(),
    createAllScenesSelector(),
    createPerformerSelector(),
    createDimensionsSelector(),
    (studioForeignId, performerScenes, studio, scenes, performer, dimensions) => {

      const scenesInStudio = scenes.filter((scene) => scene.studioForeignId === studioForeignId && scene.credits.some((credit) => credit.performer.foreignId === performer.foreignId));

      return {
        ...studio,
        items: scenesInStudio,
        columns: performerScenes.columns,
        sortKey: performerScenes.sortKey,
        sortDirection: performerScenes.sortDirection,
        performerMonitored: performer.monitored,
        path: performer.path,
        isSmallScreen: dimensions.isSmallScreen,
        isSearching: false
      };
    }
  );
}

const mapDispatchToProps = {
  setPerformerScenesSort,
  setPerformerScenesTableOption,
  toggleStudioMonitored,
  toggleMovieMonitored,
  executeCommand
};

class PerformerDetailsStudioConnector extends Component {

  onTableOptionChange = (payload) => {
    this.props.setPerformerScenesTableOption(payload);
  };

  onMonitorStudioPress = (monitored) => {
    const {
      id
    } = this.props;

    this.props.toggleStudioMonitored({
      studioId: id,
      monitored
    });
  };

  onMonitorMoviePress = (movieId, monitored) => {
    this.props.toggleMovieMonitored({
      movieId,
      monitored
    });
  };

  onSortPress = (sortKey, sortDirection) => {
    this.props.setPerformerScenesSort({
      sortKey,
      sortDirection
    });
  };

  //
  // Render

  render() {
    return (
      <PerformerDetailsStudio
        {...this.props}
        onTableOptionChange={this.onTableOptionChange}
        onSortPress={this.onSortPress}
        onMonitorStudioPress={this.onMonitorStudioPress}
        onMonitorMoviePress={this.onMonitorMoviePress}
      />
    );
  }
}

PerformerDetailsStudioConnector.propTypes = {
  id: PropTypes.number.isRequired,
  performerId: PropTypes.number.isRequired,
  setPerformerScenesTableOption: PropTypes.func.isRequired,
  setPerformerScenesSort: PropTypes.func.isRequired,
  toggleStudioMonitored: PropTypes.func.isRequired,
  toggleMovieMonitored: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(PerformerDetailsStudioConnector);
