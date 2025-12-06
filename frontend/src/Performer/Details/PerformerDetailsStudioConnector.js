import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import * as commandNames from 'Commands/commandNames';
import { sortDirections } from 'Helpers/Props';
import { executeCommand } from 'Store/Actions/commandActions';
import { bulkMonitorMovie, toggleMovieMonitored } from 'Store/Actions/movieActions';
import { setPerformerScenesSort, setPerformerScenesTableOption } from 'Store/Actions/performerScenesActions';
import { toggleStudioMonitored } from 'Store/Actions/studioActions';
import createDimensionsSelector from 'Store/Selectors/createDimensionsSelector';
import createPerformerSelector from 'Store/Selectors/createPerformerSelector';
import PerformerDetailsStudio from './PerformerDetailsStudio';

function getSortClause(sortKey, sortDirection, sortPredicates) {
  if (sortPredicates && sortPredicates.hasOwnProperty(sortKey)) {
    return function(item) {
      return sortPredicates[sortKey](item, sortDirection);
    };
  }

  return function(item) {
    return item[sortKey];
  };
}

function sort(items, state) {
  const {
    sortKey,
    sortDirection,
    sortPredicates,
    secondarySortKey,
    secondarySortDirection
  } = state;

  const clauses = [];
  const orders = [];

  clauses.push(getSortClause(sortKey, sortDirection, sortPredicates));
  orders.push(sortDirection === sortDirections.ASCENDING ? 'asc' : 'desc');

  if (secondarySortKey &&
      secondarySortDirection &&
      (sortKey !== secondarySortKey ||
       sortDirection !== secondarySortDirection)) {
    clauses.push(getSortClause(secondarySortKey, secondarySortDirection, sortPredicates));
    orders.push(secondarySortDirection === sortDirections.ASCENDING ? 'asc' : 'desc');
  }

  return _.orderBy(items, clauses, orders);
}

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
    createStudioByForeignIdSelector(),
    (state) => state.movies,
    createPerformerSelector(),
    createDimensionsSelector(),
    (state) => _.get(state, 'performerScenes'),
    (studioForeignId, studio, scenes, performer, dimensions, performerScenes) => {

      let scenesInStudio = scenes.items.filter((scene) => scene.studioForeignId === studioForeignId && scene.credits.some((credit) => credit.performer.foreignId === performer.foreignId));
      // Sort once filtered
      scenesInStudio = sort(scenesInStudio, performerScenes);

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
  bulkMonitorMovie,
  executeCommand
};

class PerformerDetailsStudioConnector extends Component {

  onTableOptionChange = (payload) => {
    this.props.setPerformerScenesTableOption(payload);
  };

  onMonitorStudioPress = (monitored) => {
    // Use the bulk monitor API to toggle monitoring for all items in this studio
    const { items } = this.props;

    const allMonitored = items.every((movie) => movie.monitored);
    const newMonitoredState = !allMonitored;
    const ids = items.map((item) => item.id);

    this.props.bulkMonitorMovie({ ids, monitored: newMonitoredState });
  };

  onMonitorMoviePress = (movieId, monitored) => {
    this.props.toggleMovieMonitored({
      movieId,
      monitored
    });
  };

  onSearchPress = () => {
    this.props.executeCommand({
      name: commandNames.PERFORMER_SEARCH,
      performerIds: [this.props.performerId],
      studioIds: [this.props.id]
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
        onSearchPress={this.onSearchPress}
        onMonitorStudioPress={this.onMonitorStudioPress}
        onMonitorMoviePress={this.onMonitorMoviePress}
      />
    );
  }
}

PerformerDetailsStudioConnector.propTypes = {
  id: PropTypes.number.isRequired,
  items: PropTypes.array.isRequired,
  performerId: PropTypes.number.isRequired,
  setPerformerScenesTableOption: PropTypes.func.isRequired,
  setPerformerScenesSort: PropTypes.func.isRequired,
  toggleStudioMonitored: PropTypes.func.isRequired,
  toggleMovieMonitored: PropTypes.func.isRequired,
  bulkMonitorMovie: PropTypes.func.isRequired,
  executeCommand: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(PerformerDetailsStudioConnector);
