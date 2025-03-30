import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import * as commandNames from 'Commands/commandNames';
import { sortDirections } from 'Helpers/Props';
import { executeCommand } from 'Store/Actions/commandActions';
import { toggleMovieMonitored } from 'Store/Actions/movieActions';
import { setStudioScenesSort, setStudioScenesTableOption } from 'Store/Actions/studioScenesActions';
import createDimensionsSelector from 'Store/Selectors/createDimensionsSelector';
import createStudioSelector from 'Store/Selectors/createStudioSelector';
import StudioDetailsYear from './StudioDetailsYear';

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

function createMapStateToProps() {
  return createSelector(
    (state, { year }) => year,
    (state, { studioForeignId }) => studioForeignId,
    (state) => state.movies,
    createStudioSelector(),
    createDimensionsSelector(),
    (state) => _.get(state, 'studioScenes'),
    (year, studioForeignId, scenes, studio, dimensions, studioScenes) => {

      let scenesInYear = scenes.items.filter((scene) => scene.studioForeignId === studioForeignId && scene.year === year);
      // Sort once filtered
      scenesInYear = sort(scenesInYear, studioScenes);

      return {
        year,
        items: scenesInYear,
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
